using Grasshopper.Kernel;
using System;

namespace NotionConnect
{
    public class HeadingBlockComponent : GH_Component
    {
        public HeadingBlockComponent()
          : base("Heading Block", "Heading",
              "Creates a Notion heading block (H1, H2, or H3).",
              "NotionConnect", "Blocks")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "T", "Heading text.", GH_ParamAccess.item, "");
            pManager.AddIntegerParameter("Style", "S", "Heading level: 1 = H1, 2 = H2, 3 = H3.", GH_ParamAccess.item, 2);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BlockJson", "B", "Heading block JSON.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string text = "";
            int style = 2;

            DA.GetData(0, ref text);
            DA.GetData(1, ref style);

            if (style < 1) style = 1;
            if (style > 3) style = 3;

            DA.SetData(0, BlockBuilders.HeadingJson(text, style));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_HeadingBlock;
        public override Guid ComponentGuid => new Guid("8A7C0360-374B-4275-BD76-13C37FCA543F");
    }
}