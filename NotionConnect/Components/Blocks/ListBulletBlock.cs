using Grasshopper.Kernel;
using System;

namespace NotionConnect
{
    public class ListBulletBlockComponent : GH_Component
    {
        public ListBulletBlockComponent()
          : base("List Bullet Block", "List Bullet",
              "Creates a Notion bulleted list item block.",
              "NotionConnect", "Blocks")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "T", "Item text.", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BlockJson", "B", "Bulleted list item block JSON.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string text = "";
            DA.GetData(0, ref text);
            DA.SetData(0, BlockBuilders.BulletedItemJson(text));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_ListBulletBlock;
        public override Guid ComponentGuid => new Guid("BB829B97-6797-4B8C-B394-AE976F23086B");
    }
}