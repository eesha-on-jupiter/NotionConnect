using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace NotionConnect
{
    public class DatabaseAssemblerComponent : GH_Component
    {
        public DatabaseAssemblerComponent()
          : base("Database Assembler", "DB Assembler",
              "Assembles one row JSON per branch from property definition and value trees.",
              "NotionConnect", "Database")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Database ID", "DID", "Target Notion database ID.", GH_ParamAccess.item);
            pManager.AddTextParameter("HeadingJson", "HJ", "Property definition JSONs — one per column (from HJ outputs).", GH_ParamAccess.list);
            pManager.AddTextParameter("RowJson", "RJ", "Row value JSON tree — one branch per row, one item per property (from RJ outputs).", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("RowJson", "RJ", "Row create JSON payloads — one per row.", GH_ParamAccess.list);
            pManager.AddTextParameter("Error", "E", "Warnings and errors per row.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string databaseId = null;
            var propDefs = new List<string>();
            GH_Structure<GH_String> rowValuesTree;

            if (!DA.GetData(0, ref databaseId)) return;
            DA.GetDataList(1, propDefs);
            DA.GetDataTree(2, out rowValuesTree);

            if (string.IsNullOrWhiteSpace(databaseId)) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Database ID is required."); return; }
            if (propDefs.Count == 0) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "HeadingJson list is empty."); return; }

            var propNames = new List<string>();
            for (int i = 0; i < propDefs.Count; i++)
            {
                string propName = null;
                try { propName = JObject.Parse(propDefs[i])["name"]?.ToString(); } catch { }
                propNames.Add(string.IsNullOrWhiteSpace(propName) ? $"Property{i}" : propName);
            }

            var rowJsons = new List<string>();
            var errorLines = new List<string>();
            var branches = rowValuesTree.Branches;

            if (branches.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "RowJson tree has no branches — no rows to assemble.");
                DA.SetDataList(0, rowJsons);
                DA.SetData(1, "No branches in RJ tree.");
                return;
            }

            for (int rowIdx = 0; rowIdx < branches.Count; rowIdx++)
            {
                var branch = branches[rowIdx];
                var props = new Dictionary<string, JObject>();
                int pairCount = Math.Min(propNames.Count, branch.Count);

                if (propNames.Count != branch.Count)
                    errorLines.Add($"Row {rowIdx}: {propNames.Count} props but {branch.Count} values — using first {pairCount} pairs.");

                for (int colIdx = 0; colIdx < pairCount; colIdx++)
                {
                    string propName = propNames[colIdx];
                    string valJson = branch[colIdx]?.Value;

                    if (string.IsNullOrWhiteSpace(valJson)) { errorLines.Add($"Row {rowIdx}, '{propName}': empty value JSON, skipping."); continue; }

                    try { props[propName] = JObject.Parse(valJson); }
                    catch { errorLines.Add($"Row {rowIdx}, '{propName}': invalid JSON, skipping."); }
                }

                try { rowJsons.Add(DatabaseRowBuilders.CreateRowJson(databaseId, props)); }
                catch (Exception ex) { errorLines.Add($"Row {rowIdx}: failed to build JSON — {ex.Message}"); }
            }

            DA.SetDataList(0, rowJsons);
            DA.SetData(1, errorLines.Count == 0 ? $"OK — {rowJsons.Count} rows assembled." : string.Join("\n", errorLines));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_DatabaseAssembler;
        public override Guid ComponentGuid => new Guid("C41E3C2A-8B14-4B41-ABAF-6A61043E6FC2");
    }
}