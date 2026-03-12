using Grasshopper.Kernel;
using System;

namespace NotionConnect
{
    public class CodeBlockComponent : GH_Component
    {
        public CodeBlockComponent()
          : base("Code Block", "Code",
              "Creates a Notion code block.",
              "NotionConnect", "Blocks")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Code", "C", "Code content.", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Language", "L", "Code language (e.g. csharp, python, javascript).", GH_ParamAccess.item, "plain text");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BlockJson", "B", "Code block JSON.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string code = "";
            string language = "plain text";

            DA.GetData(0, ref code);
            DA.GetData(1, ref language);

            DA.SetData(0, BlockBuilders.CodeJson(code, language));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_CodeBlock;
        public override Guid ComponentGuid => new Guid("344B5D94-ED8D-47E6-920A-B823D3BAFBBA");
    }
}