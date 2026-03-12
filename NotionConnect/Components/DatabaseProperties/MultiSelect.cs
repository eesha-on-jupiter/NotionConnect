using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;

namespace NotionConnect
{
    public class MultiSelectPropComponent : GH_Component
    {
        public MultiSelectPropComponent()
          : base("Multi Select Prop", "Multi Select",
              "Creates a Notion database Multi Select property.",
              "NotionConnect", "DatabaseProperties")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Header", "H", "Property name (column header in Notion database).", GH_ParamAccess.item, "Tags");
            pManager.AddTextParameter("Options", "O", "Option names available in the multi-select menu.", GH_ParamAccess.list);
            pManager.AddTextParameter("Colors", "C", "Optional colors per option (same order as Options).", GH_ParamAccess.list);
            pManager.AddTextParameter("Values", "V", "Selected tags per row — connect as tree. Each branch = one row, each item = one selected tag.", GH_ParamAccess.tree);
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("HeadingJson", "HJ", "Property definition JSON for the database schema.", GH_ParamAccess.item);
            pManager.AddTextParameter("RowJson", "RJ", "Row value JSONs — one branch per row.", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "Tags";
            var options = new List<string>();
            var colors = new List<string>();

            DA.GetData(0, ref name);
            if (!DA.GetDataList(1, options)) return;
            DA.GetDataList(2, colors);
            DA.GetDataTree(3, out GH_Structure<GH_String> ghTree);

            if (string.IsNullOrWhiteSpace(name)) name = "Unnamed";
            if (options.Count == 0) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Provide at least one option."); return; }

            DA.SetData(0, DatabasePropertyBuilders.MultiSelect(name, options, colors));
            var tree = new Grasshopper.DataTree<string>();
            for (int i = 0; i < ghTree.Branches.Count; i++)
            {
                var selected = new List<string>();
                foreach (var gh in ghTree.Branches[i])
                    if (gh != null) selected.Add(gh.Value);
                tree.Add(DatabaseRowBuilders.MultiSelectValue(selected).ToString(), new GH_Path(i));
            }
            DA.SetDataTree(1, tree);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_MultiSelectProp;
        public override Guid ComponentGuid => new Guid("B3517443-A4D3-4DE9-AC77-B07B276A3E39");
    }
}