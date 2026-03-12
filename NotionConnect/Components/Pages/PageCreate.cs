using Grasshopper.Kernel;
using System;

namespace NotionConnect
{
    public class CreatePageComponent : ButtonComponent
    {
        public CreatePageComponent()
          : base("Create Page", "Create Page",
              "Creates a new Notion page as a child of an existing page. Press the button to send.",
              "NotionConnect", "Pages")
        { }

        public override string ButtonLabel => "Create";

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Notion internal integration token.", GH_ParamAccess.item);
            pManager.AddTextParameter("Page ID", "PID", "Parent page ID.", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "Name of the new page.", GH_ParamAccess.item, "New Page");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Page ID", "PID", "ID of the newly created page.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("OK", "OK", "True if request succeeded.", GH_ParamAccess.item);
            pManager.AddTextParameter("Error", "E", "Error message, if any.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string token = "";
            string parentId = "";
            string title = "New Page";

            if (!DA.GetData(0, ref token)) { DA.SetData(1, false); DA.SetData(2, "Token missing."); return; }
            if (!DA.GetData(1, ref parentId)) { DA.SetData(1, false); DA.SetData(2, "Page ID missing."); return; }
            DA.GetData(2, ref title);

            if (string.IsNullOrWhiteSpace(token)) { DA.SetData(1, false); DA.SetData(2, "Token is empty."); return; }
            if (string.IsNullOrWhiteSpace(parentId)) { DA.SetData(1, false); DA.SetData(2, "Page ID is empty."); return; }
            if (string.IsNullOrWhiteSpace(title)) title = "New Page";

            if (!IsTriggered) return;

            try
            {
                var client = new NotionClient(token);
                var result = client.CreatePageAsync(parentId, title).GetAwaiter().GetResult();

                DA.SetData(1, result.Item1);
                DA.SetData(2, result.Item3);

                if (result.Item1)
                {
                    string pageId = Newtonsoft.Json.Linq.JObject.Parse(result.Item2)?["id"]?.ToString();
                    DA.SetData(0, pageId);
                }
            }
            catch (Exception ex)
            {
                DA.SetData(1, false);
                DA.SetData(2, ex.ToString());
            }
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_PageCreate;
        public override Guid ComponentGuid => new Guid("1163AD10-BE55-4D2B-8D11-65571F0028F3");
    }
}