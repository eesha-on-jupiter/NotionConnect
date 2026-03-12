using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;

namespace NotionConnect
{
    public class TableBlockComponent : GH_Component
    {
        public TableBlockComponent()
          : base("Table Block", "Table",
              "Creates a Notion table block. Each tree path = one row, each item = one cell.",
              "NotionConnect", "Blocks")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Cells", "C", "Data tree of cell strings. Each path = row, each item = column.", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Width", "W", "Column count override. Set to 0 to auto-detect from data.", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("ColumnHeader", "CH", "If true, first row is styled as a header row.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("RowHeader", "RH", "If true, first column is styled as a header column.", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BlockJson", "B", "Table block JSON.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("AutoWidth", "AW", "Computed column count used.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("RowCount", "R", "Number of rows used.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_String> tree;
            int width = 0;
            bool colHeader = false;
            bool rowHeader = false;

            if (!DA.GetDataTree(0, out tree)) return;
            DA.GetData(1, ref width);
            DA.GetData(2, ref colHeader);
            DA.GetData(3, ref rowHeader);

            var rows = new List<List<string>>();
            int autoWidth = 1;

            foreach (var path in tree.Paths)
            {
                var branch = tree.get_Branch(path);
                var row = new List<string>();
                for (int i = 0; i < branch.Count; i++)
                {
                    var ghStr = branch[i] as GH_String;
                    row.Add(ghStr?.Value ?? "");
                }
                rows.Add(row);
                if (row.Count > autoWidth) autoWidth = row.Count;
            }

            int finalWidth = (width > 0) ? width : autoWidth;
            if (finalWidth < 1) finalWidth = 1;

            DA.SetData(0, BlockBuilders.TableJson(rows, finalWidth, colHeader, rowHeader));
            DA.SetData(1, finalWidth);
            DA.SetData(2, rows.Count);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_TableBlock;
        public override Guid ComponentGuid => new Guid("B57A146D-1E30-4FA1-9461-DABFA401FCAE");
    }
}