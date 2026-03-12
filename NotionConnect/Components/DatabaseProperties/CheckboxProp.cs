using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using System;
using System.Collections.Generic;

namespace NotionConnect
{
    public class CheckboxPropComponent : GH_Component
    {
        public CheckboxPropComponent()
          : base("Checkbox Prop", "Checkbox",
              "Creates a Notion database Checkbox property.",
              "NotionConnect", "DatabaseProperties")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Header", "H", "Property name (column header in Notion database).", GH_ParamAccess.item, "Done");
            pManager.AddBooleanParameter("Values", "V", "Boolean row values — one per row.", GH_ParamAccess.list, true);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("HeadingJson", "HJ", "Property definition JSON for the database schema.", GH_ParamAccess.item);
            pManager.AddTextParameter("RowJson", "RJ", "Row value JSONs — one branch per row.", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "Done";
            var values = new List<bool>();
            DA.GetData(0, ref name);
            DA.GetDataList(1, values);
            if (string.IsNullOrWhiteSpace(name)) name = "Unnamed";
            DA.SetData(0, DatabasePropertyBuilders.Checkbox(name));
            var tree = new Grasshopper.DataTree<string>();
            for (int i = 0; i < values.Count; i++)
                tree.Add(DatabaseRowBuilders.CheckboxValue(values[i]).ToString(), new GH_Path(i));
            DA.SetDataTree(1, tree);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_CheckboxProp;
        public override Guid ComponentGuid => new Guid("53272715-2EC4-40A2-88B5-5FF6364CB77F");
    }
}