using Grasshopper.Kernel;
using System;

namespace NotionConnect
{
    public class TextBlockComponent : GH_Component
    {
        public TextBlockComponent()
          : base("Text Block", "Text",
              "Creates a Notion paragraph block.",
              "NotionConnect", "Blocks")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "T", "Paragraph text.", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BlockJson", "B", "Paragraph block JSON.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string text = "";
            DA.GetData(0, ref text);
            DA.SetData(0, BlockBuilders.ParagraphJson(text));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_TextBlock;
        public override Guid ComponentGuid => new Guid("FC288523-F1A5-435C-AF01-ED4851D08C20");
    }
}