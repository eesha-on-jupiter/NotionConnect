using Grasshopper.Kernel;
using System;

namespace NotionConnect
{
    public class DividerBlockComponent : GH_Component
    {
        public DividerBlockComponent()
          : base("Divider Block", "Divider",
              "Creates a Notion divider block.",
              "NotionConnect", "Blocks")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager) { }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BlockJson", "B", "Divider block JSON.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData(0, BlockBuilders.DividerJson());
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_DivideBlock;
        public override Guid ComponentGuid => new Guid("AE4EB344-95DB-48DB-8E78-169AEBE631E6");
    }
}