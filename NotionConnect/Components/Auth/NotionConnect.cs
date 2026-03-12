using System;
using Grasshopper.Kernel;

namespace NotionConnect
{
    public class NotionConnectComponent : GH_Component
    {
        public NotionConnectComponent()
          : base("Notion Connect", "Connect",
              "Validates a Notion integration token by calling /users/me.",
              "NotionConnect", "Auth")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Notion internal integration token.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("OK", "OK", "True if token is valid.", GH_ParamAccess.item);
            pManager.AddTextParameter("User", "U", "Notion user or bot name.", GH_ParamAccess.item);
            pManager.AddTextParameter("Error", "E", "Error message, if any.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string token = "";
            if (!DA.GetData(0, ref token) || string.IsNullOrWhiteSpace(token))
            {
                DA.SetData(0, false);
                DA.SetData(1, "");
                DA.SetData(2, "Token is empty.");
                return;
            }

            var client = new NotionClient(token);
            var result = client.GetMeAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            DA.SetData(0, result.Item1);
            DA.SetData(1, result.Item2);
            DA.SetData(2, result.Item3);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_NotionConnect;
        public override Guid ComponentGuid => new Guid("ADA68EBD-7014-410C-B67F-2C90FB1CF21D");
    }
}