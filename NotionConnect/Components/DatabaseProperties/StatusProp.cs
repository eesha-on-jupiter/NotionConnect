using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace NotionConnect
{
    public class StatusPropComponent : GH_Component
    {
        public StatusPropComponent()
          : base("Status Prop", "Status",
              "Creates a Notion database Status property.",
              "NotionConnect", "DatabaseProperties")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Header", "H", "Property name (column header in Notion database).", GH_ParamAccess.item, "Status");
            pManager.AddTextParameter("Options", "O", "Status option names (e.g. Not Started, In Progress).", GH_ParamAccess.list);
            pManager.AddTextParameter("Colors", "C", "Optional colors per option (same order as Options).", GH_ParamAccess.list);
            pManager.AddTextParameter("Values", "V", "Selected status names — one per row.", GH_ParamAccess.list, "");
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

            DA.SetData(0, DatabasePropertyBuilders.Status(name, options, colors));
            var tree = new Grasshopper.DataTree<string>();
            for (int i = 0; i < values.Count; i++)
            {
                var rowVal = new JObject { ["status"] = new JObject { ["name"] = values[i] ?? "" } };
                tree.Add(rowVal.ToString(), new GH_Path(i));
            }
            DA.SetDataTree(1, tree);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_StatusProp;
        public override Guid ComponentGuid => new Guid("E4F5A6B7-C8D9-0123-EFAB-234567890004");
    }
}