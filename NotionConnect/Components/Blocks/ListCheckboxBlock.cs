using Grasshopper.Kernel;
using System;

namespace NotionConnect
{
    public class ListChecklistBlockComponent : GH_Component
    {
        public ListChecklistBlockComponent()
          : base("List Checklist Block", "List Checklist",
              "Creates a Notion to-do checklist item block.",
              "NotionConnect", "Blocks")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "T", "Item text.", GH_ParamAccess.item, "");
            pManager.AddBooleanParameter("Checked", "X", "Checked state.", GH_ParamAccess.item, false);
            pManager.AddTextParameter("Color", "C", "Notion color.", GH_ParamAccess.item, "default");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BlockJson", "B", "Checklist item block JSON.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string text = "";
            bool isChecked = false;
            string color = "default";

            DA.GetData(0, ref text);
            DA.GetData(1, ref isChecked);
            DA.GetData(2, ref color);

            DA.SetData(0, BlockBuilders.ToDoJson(text, isChecked, color));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_ListCheckboxBlock;
        public override Guid ComponentGuid => new Guid("E1CDFBA0-7D8B-4B8C-973D-F53F455F0DB2");
    }
}