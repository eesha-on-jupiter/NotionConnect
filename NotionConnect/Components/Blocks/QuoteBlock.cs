using Grasshopper.Kernel;
using System;

namespace NotionConnect
{
    public class QuoteBlockComponent : GH_Component
    {
        public QuoteBlockComponent()
          : base("Quote Block", "Quote",
              "Creates a Notion quote block.",
              "NotionConnect", "Blocks")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "T", "Quote text.", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BlockJson", "B", "Quote block JSON.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string text = "";
            DA.GetData(0, ref text);
            DA.SetData(0, BlockBuilders.QuoteJson(text));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_QuoteBlock;
        public override Guid ComponentGuid => new Guid("737EED0B-EACF-4327-91D4-D364A032DB09");
    }
}