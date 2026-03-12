using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace NotionConnect.Components.Database
{
    public class DatabaseRowAppendComponent : ButtonComponent
    {
        public override string ButtonLabel => "Append";

        public DatabaseRowAppendComponent()
          : base("Database Append Rows", "DB Append Rows",
              "Appends new rows to an existing Notion database. Press the button to send.",
              "NotionConnect", "Database")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Notion internal integration token.", GH_ParamAccess.item);
            pManager.AddTextParameter("RowJson", "RJ", "Row JSON payloads — from Database Assembler.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Page IDs", "PID", "Created page IDs — one per row.", GH_ParamAccess.list);
            pManager.AddTextParameter("Error", "E", "Error per row (empty if OK).", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string token = null;
            var rowJsons = new List<string>();

            if (!DA.GetData(0, ref token)) return;
            DA.GetDataList(1, rowJsons);

            token = token?.Trim();

            if (!IsTriggered) return;

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

        protected override Bitmap Icon => Properties.Resources.NC_DBRowAppend;
        public override Guid ComponentGuid => new Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567892");
    }
}