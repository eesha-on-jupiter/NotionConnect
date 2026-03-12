using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using System;
using System.Collections.Generic;

namespace NotionConnect
{
    public class EmailPropComponent : GH_Component
    {
        public EmailPropComponent()
          : base("Email Prop", "Email",
              "Creates a Notion database Email property.",
              "NotionConnect", "DatabaseProperties")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Header", "H", "Property name (column header in Notion database).", GH_ParamAccess.item, "Email");
            pManager.AddTextParameter("Values", "V", "Email strings — one per row.", GH_ParamAccess.list, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("HeadingJson", "HJ", "Property definition JSON for the database schema.", GH_ParamAccess.item);
            pManager.AddTextParameter("RowJson", "RJ", "Row value JSONs — one branch per row.", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "Email";
            var values = new List<string>();
            DA.GetData(0, ref name);
            DA.GetDataList(1, values);
            if (string.IsNullOrWhiteSpace(name)) name = "Unnamed";
            DA.SetData(0, DatabasePropertyBuilders.Email(name));
            var tree = new Grasshopper.DataTree<string>();
            for (int i = 0; i < values.Count; i++)
                tree.Add(DatabaseRowBuilders.EmailValue(values[i]).ToString(), new GH_Path(i));
            DA.SetDataTree(1, tree);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_EmailProp;
        public override Guid ComponentGuid => new Guid("C2D3E4F5-A6B7-8901-CDEF-012345678902");
    }
}