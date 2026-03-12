using Grasshopper.Kernel;
using System;

namespace NotionConnect
{
    public class CalloutBlockComponent : GH_Component
    {
        public CalloutBlockComponent()
          : base("Callout Block", "Callout",
              "Creates a Notion callout block.",
              "NotionConnect", "Blocks")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "T", "Callout text.", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Emoji", "M", "Emoji icon (e.g. 💡).", GH_ParamAccess.item, "💡");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BlockJson", "B", "Callout block JSON.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string text = "";
            string emoji = "💡";

            DA.GetData(0, ref text);
            DA.GetData(1, ref emoji);

            if (string.IsNullOrWhiteSpace(emoji)) emoji = "💡";

            DA.SetData(0, BlockBuilders.CalloutJson(text, emoji));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_CalloutBlock;
        public override Guid ComponentGuid => new Guid("F847A71E-FCE9-4FD5-925E-3A00D48DBDCE");
    }
}