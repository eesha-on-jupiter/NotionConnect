using Grasshopper.Kernel;
using System;

namespace NotionConnect
{
    public class ListNumberBlockComponent : GH_Component
    {
        public ListNumberBlockComponent()
          : base("List Number Block", "List Number",
              "Creates a Notion numbered list item block.",
              "NotionConnect", "Blocks")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "T", "Item text.", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BlockJson", "B", "Numbered list item block JSON.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string text = "";
            DA.GetData(0, ref text);
            DA.SetData(0, BlockBuilders.NumberedItemJson(text));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_ListNumberBlock;
        public override Guid ComponentGuid => new Guid("5ADA49CC-754D-4205-8DA7-1501EAAA3BD4");
    }
}