using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace NotionConnect.Components.Database
{
    public class DatabaseRowAppendHeadlessComponent : GH_Component
    {
        public DatabaseRowAppendHeadlessComponent()
          : base("DB Append Rows Headless", "DB Append Headless",
              "Appends new rows to an existing Notion database. Send is triggered by a boolean toggle — compatible with ShapeDiver and other headless environments.",
              "NotionConnect", "Headless")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Notion internal integration token.", GH_ParamAccess.item);
            pManager.AddTextParameter("RowJson", "RJ", "Row JSON payloads — from Database Assembler.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Send", "S", "Set to True to trigger the append. Fires on False→True transition only.", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Page IDs", "PID", "Created page IDs — one per row.", GH_ParamAccess.list);
            pManager.AddTextParameter("Error", "E", "Error per row (empty if OK).", GH_ParamAccess.list);
        }

        private bool _lastSend = false;

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string token = null;
            var rowJsons = new List<string>();
            bool send = false;

            if (!DA.GetData(0, ref token)) return;
            DA.GetDataList(1, rowJsons);
            DA.GetData(2, ref send);

            token = token?.Trim();

            bool rising = send && !_lastSend;
            _lastSend = send;

            if (!rising) return;

            if (string.IsNullOrWhiteSpace(token)) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Token is required."); return; }
            if (rowJsons.Count == 0) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No row JSONs provided."); return; }

            var client = new NotionClient(token);
            var pageIds = new string[rowJsons.Count];
            var errors = new string[rowJsons.Count];

            try
            {
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

        protected override Bitmap Icon => Properties.Resources.NC_HeadlessDBAppend;
        public override Guid ComponentGuid => new Guid("67F1A117-0A8A-4315-807E-795237E40FF0");
    }
}