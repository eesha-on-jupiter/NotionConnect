using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace NotionConnect
{
    public class TableOfContentsBlockComponent : GH_Component
    {
        public TableOfContentsBlockComponent()
          : base("Table Of Contents Block", "TOC",
              "Creates a Notion table of contents block — auto-generates navigation from page headings.",
              "NotionConnect", "Blocks")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Color", "C", "Notion color (default, gray, blue, etc.).", GH_ParamAccess.item, "default");
            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BlockJson", "B", "Table of contents block JSON.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string color = "default";
            DA.GetData(0, ref color);
            DA.SetData(0, BlockBuilders.TableOfContentsJson(color));
        }

        protected override Bitmap Icon => Properties.Resources.NC_TableOfContentsBlock;
        public override Guid ComponentGuid => new Guid("A1B2C3D4-1111-2222-3333-000000000005");
    }
}