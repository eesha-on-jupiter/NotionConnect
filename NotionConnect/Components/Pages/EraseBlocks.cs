using Grasshopper.Kernel;
using System;

namespace NotionConnect
{
    public class ErasePageComponent : ButtonComponent
    {
        public ErasePageComponent()
          : base("Erase Page", "ErasePage",
              "Erases all blocks on a Notion page. Press the button to send.",
              "NotionConnect", "Pages")
        { }

        public override string ButtonLabel => "Erase";

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Notion internal integration token.", GH_ParamAccess.item);
            pManager.AddTextParameter("Page ID", "PID", "Target Notion page ID.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("OK", "OK", "True if request succeeded.", GH_ParamAccess.item);
            pManager.AddTextParameter("Error", "E", "Error message, if any.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string token = "";
            string pageId = "";

            if (!DA.GetData(0, ref token)) { DA.SetData(0, false); DA.SetData(1, "Token missing."); return; }
            if (!DA.GetData(1, ref pageId)) { DA.SetData(0, false); DA.SetData(1, "Page ID missing."); return; }
            if (string.IsNullOrWhiteSpace(token)) { DA.SetData(0, false); DA.SetData(1, "Token is empty."); return; }
            if (string.IsNullOrWhiteSpace(pageId)) { DA.SetData(0, false); DA.SetData(1, "Page ID empty."); return; }

            if (!IsTriggered) return;

            try
            {
                var client = new NotionClient(token);
                var result = client.EraseContentOnPageAsync(pageId).GetAwaiter().GetResult();

                DA.SetData(0, result.Item1);
                DA.SetData(1, result.Item3);
            }
            catch (Exception ex)
            {
                DA.SetData(0, false);
                DA.SetData(1, ex.ToString());
            }
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_EraseBlock;
        public override Guid ComponentGuid => new Guid("FCE82C26-46F1-4F68-A4C7-6458BFBD1D94");
    }
}