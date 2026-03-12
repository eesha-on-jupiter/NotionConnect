using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using System;
using System.Collections.Generic;

namespace NotionConnect
{
    public class DatePropComponent : GH_Component
    {
        public DatePropComponent()
          : base("Date Prop", "Date",
              "Creates a Notion database Date property.",
              "NotionConnect", "DatabaseProperties")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Header", "H", "Property name (column header in Notion database).", GH_ParamAccess.item, "Date");
            pManager.AddTextParameter("Values", "V", "ISO 8601 date strings — one per row (e.g. 2025-03-05).", GH_ParamAccess.list, "2025-01-01");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("HeadingJson", "HJ", "Property definition JSON for the database schema.", GH_ParamAccess.item);
            pManager.AddTextParameter("RowJson", "RJ", "Row value JSONs — one branch per row.", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "Date";
            var values = new List<string>();
            DA.GetData(0, ref name);
            DA.GetDataList(1, values);
            if (string.IsNullOrWhiteSpace(name)) name = "Unnamed";
            DA.SetData(0, DatabasePropertyBuilders.Date(name));
            var tree = new Grasshopper.DataTree<string>();
            for (int i = 0; i < values.Count; i++)
                tree.Add(DatabaseRowBuilders.DateValue(values[i]).ToString(), new GH_Path(i));
            DA.SetDataTree(1, tree);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_DateProp;
        public override Guid ComponentGuid => new Guid("CA5940AC-AC4C-4B21-81BB-893032E9D68B");
    }
}