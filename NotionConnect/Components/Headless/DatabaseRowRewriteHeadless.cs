using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace NotionConnect.Components.Database
{
    public class DatabaseRowRewriteHeadlessComponent : GH_Component
    {
        public DatabaseRowRewriteHeadlessComponent()
          : base("DB Rewrite Rows Headless", "DB Rewrite Headless",
              "Archives all existing rows then posts new ones. Send is triggered by a boolean toggle — compatible with ShapeDiver and other headless environments.",
              "NotionConnect", "Headless")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Notion internal integration token.", GH_ParamAccess.item);
            pManager.AddTextParameter("Database ID", "DID", "Target Notion database ID.", GH_ParamAccess.item);
            pManager.AddTextParameter("RowJson", "RJ", "Row JSON payloads — from Database Assembler.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Send", "S", "Set to True to trigger the rewrite. Fires on False→True transition only.", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Page IDs", "PID", "Created page IDs — one per new row.", GH_ParamAccess.list);
            pManager.AddTextParameter("Error", "E", "Error per row (empty if OK).", GH_ParamAccess.list);
        }

        private bool _lastSend = false;

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string token = null;
            string databaseId = null;
            var rowJsons = new List<string>();
            bool send = false;

            if (!DA.GetData(0, ref token)) return;
            if (!DA.GetData(1, ref databaseId)) return;
            DA.GetDataList(2, rowJsons);
            DA.GetData(3, ref send);

            token = token?.Trim();
            databaseId = databaseId?.Trim();

            bool rising = send && !_lastSend;
            _lastSend = send;

            if (!rising) return;

            if (string.IsNullOrWhiteSpace(token)) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Token is required."); return; }
            if (string.IsNullOrWhiteSpace(databaseId)) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Database ID is required."); return; }
            if (rowJsons.Count == 0) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No row JSONs provided."); return; }

            var client = new NotionClient(token);
            try
            {
                // STEP 1 — Query existing rows
                var queryRes = client.QueryDatabaseAsync(databaseId).GetAwaiter().GetResult();
                if (!queryRes.Item1)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Query failed: " + queryRes.Item3);
                    return;
                }

                var existingIds = queryRes.Item2;

                // STEP 2 — Archive existing rows in parallel batches of 3
                if (existingIds.Count > 0)
                {
                    foreach (var batch in Batch(existingIds, 3))
                    {
                        var results = Task.WhenAll(batch.Select(id => client.ArchivePageAsync(id))).GetAwaiter().GetResult();
                        Task.Delay(400).GetAwaiter().GetResult();
                    }
                }

                // STEP 3 — Post new rows sequentially to preserve order
                var pageIds = new string[rowJsons.Count];
                var errors = new string[rowJsons.Count];

                for (int i = 0; i < rowJsons.Count; i++)
                {
                    string rowJson = rowJsons[i];
                    if (string.IsNullOrWhiteSpace(rowJson))
                    {
                        pageIds[i] = "";
                        errors[i] = "Empty row JSON — skipped.";
                        continue;
                    }

                    var r = client.CreateRowAsync(rowJson).GetAwaiter().GetResult();
                    if (r.Item1) { pageIds[i] = DatabaseRowBuilders.ParseRowPageId(r.Item2) ?? ""; errors[i] = ""; }
                    else { pageIds[i] = ""; errors[i] = r.Item3; }

                    if (i < rowJsons.Count - 1)
                        Task.Delay(350).GetAwaiter().GetResult();
                }

                DA.SetDataList(0, pageIds);
                DA.SetDataList(1, errors);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
            }
        }

        private static IEnumerable<List<T>> Batch<T>(IEnumerable<T> source, int size)
        {
            var batch = new List<T>(size);
            foreach (var item in source)
            {
                batch.Add(item);
                if (batch.Count == size) { yield return batch; batch = new List<T>(size); }
            }
            if (batch.Count > 0) yield return batch;
        }

        protected override Bitmap Icon => Properties.Resources.NC_HeadlessDBRewrite;
        public override Guid ComponentGuid => new Guid("C0E650AC-F98F-448C-B2E8-54FDFDBC4D88");
    }
}