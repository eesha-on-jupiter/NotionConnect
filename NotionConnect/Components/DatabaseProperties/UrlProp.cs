using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using System;
using System.Collections.Generic;

namespace NotionConnect
{
    public class UrlPropComponent : GH_Component
    {
        public UrlPropComponent()
          : base("Url Prop", "Url",
              "Creates a Notion database URL property.",
              "NotionConnect", "DatabaseProperties")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Header", "H", "Property name (column header in Notion database).", GH_ParamAccess.item, "URL");
            pManager.AddTextParameter("Values", "V", "URL strings — one per row.", GH_ParamAccess.list, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("HeadingJson", "HJ", "Property definition JSON for the database schema.", GH_ParamAccess.item);
            pManager.AddTextParameter("RowJson", "RJ", "Row value JSONs — one branch per row.", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "URL";
            var values = new List<string>();
            DA.GetData(0, ref name);
            DA.GetDataList(1, values);
            if (string.IsNullOrWhiteSpace(name)) name = "Unnamed";
            DA.SetData(0, DatabasePropertyBuilders.Url(name));
            var tree = new Grasshopper.DataTree<string>();
            for (int i = 0; i < values.Count; i++)
                tree.Add(DatabaseRowBuilders.UrlValue(values[i]).ToString(), new GH_Path(i));
            DA.SetDataTree(1, tree);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_UrlProp;
        public override Guid ComponentGuid => new Guid("B1C2D3E4-F5A6-7890-BCDE-F12345678901");
    }
}