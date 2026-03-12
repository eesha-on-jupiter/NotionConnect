using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using System;
using System.Collections.Generic;

namespace NotionConnect
{
    public class NumberPropComponent : GH_Component
    {
        public NumberPropComponent()
          : base("Number Prop", "Number",
              "Creates a Notion database Number property.",
              "NotionConnect", "DatabaseProperties")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Header", "H", "Property name (column header in Notion database).", GH_ParamAccess.item, "Value");
            pManager.AddTextParameter("Format", "F", "Number format (e.g. number, percent, dollar, rupee).", GH_ParamAccess.item, "number");
            pManager.AddNumberParameter("Values", "V", "Numeric row values — one per row.", GH_ParamAccess.list, 0.0);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("HeadingJson", "HJ", "Property definition JSON for the database schema.", GH_ParamAccess.item);
            pManager.AddTextParameter("RowJson", "RJ", "Row value JSONs — one branch per row.", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "Value";
            string format = "number";
            var values = new List<double>();

            DA.GetData(0, ref name);
            DA.GetData(1, ref format);
            DA.GetDataList(2, values);

            if (string.IsNullOrWhiteSpace(name)) name = "Unnamed";
            if (string.IsNullOrWhiteSpace(format)) format = "number";

            DA.SetData(0, DatabasePropertyBuilders.Number(name, format));
            var tree = new Grasshopper.DataTree<string>();
            for (int i = 0; i < values.Count; i++)
                tree.Add(DatabaseRowBuilders.NumberValue(values[i]).ToString(), new GH_Path(i));
            DA.SetDataTree(1, tree);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_NumberProp;
        public override Guid ComponentGuid => new Guid("CE5CE330-0975-4A83-9D48-373350E9F2BB");
    }
}