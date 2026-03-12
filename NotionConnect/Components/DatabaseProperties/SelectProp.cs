using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using System;
using System.Collections.Generic;

namespace NotionConnect
{
    public class SelectPropComponent : GH_Component
    {
        public SelectPropComponent()
          : base("Select Prop", "Select",
              "Creates a Notion database Select property.",
              "NotionConnect", "DatabaseProperties")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Header", "H", "Property name (column header in Notion database).", GH_ParamAccess.item, "Status");
            pManager.AddTextParameter("Options", "O", "Option names available in the select menu.", GH_ParamAccess.list);
            pManager.AddTextParameter("Colors", "C", "Optional colors per option (same order as Options).", GH_ParamAccess.list);
            pManager.AddTextParameter("Values", "V", "Selected option names — one per row.", GH_ParamAccess.list, "");
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("HeadingJson", "HJ", "Property definition JSON for the database schema.", GH_ParamAccess.item);
            pManager.AddTextParameter("RowJson", "RJ", "Row value JSONs — one branch per row.", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "Status";
            var options = new List<string>();
            var colors = new List<string>();
            var values = new List<string>();

            DA.GetData(0, ref name);
            if (!DA.GetDataList(1, options)) return;
            DA.GetDataList(2, colors);
            DA.GetDataList(3, values);

            if (string.IsNullOrWhiteSpace(name)) name = "Unnamed";
            if (options.Count == 0) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Provide at least one option."); return; }

            DA.SetData(0, DatabasePropertyBuilders.Select(name, options, colors));
            var tree = new Grasshopper.DataTree<string>();
            for (int i = 0; i < values.Count; i++)
                tree.Add(DatabaseRowBuilders.SelectValue(values[i]).ToString(), new GH_Path(i));
            DA.SetDataTree(1, tree);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_SelectProp;
        public override Guid ComponentGuid => new Guid("D88857C5-B390-4484-8A24-69391EC7FBAD");
    }
}