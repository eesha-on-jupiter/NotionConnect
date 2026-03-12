using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace NotionConnect.Utility
{
    public class ReaderCsvComponent : GH_Component
    {
        public ReaderCsvComponent()
          : base("Reader CSV", "Reader CSV",
              "Reads a CSV file and outputs a column-per-branch tree. Wire columns directly into Database property inputs.",
              "NotionConnect", "Database")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File Path", "FP", "Full path to the CSV file.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Has Headers", "H", "True if the first row contains column headers.", GH_ParamAccess.item, true);
            pManager.AddTextParameter("Delimiter", "DL", "Column delimiter character. Default is comma.", GH_ParamAccess.item, ",");
            pManager.AddTextParameter("Encoding", "ENC", "File encoding: UTF8, ASCII, Unicode, Latin1.", GH_ParamAccess.item, "UTF8");
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Columns", "C", "Column values as a tree — one branch per column, one item per row.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Headers", "H", "Column header names — parallel to Columns branches.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Rows", "R", "Number of data rows read.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Cols", "CO", "Number of columns read.", GH_ParamAccess.item);
            pManager.AddTextParameter("Error", "E", "Error message, if any.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string filePath = "";
            bool hasHeaders = true;
            string delimiter = ",";
            string encodingId = "UTF8";

            if (!DA.GetData(0, ref filePath)) return;
            DA.GetData(1, ref hasHeaders);
            DA.GetData(2, ref delimiter);
            DA.GetData(3, ref encodingId);

            filePath = filePath?.Trim();
            delimiter = string.IsNullOrEmpty(delimiter) ? "," : delimiter;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                DA.SetData(4, "File path is empty.");
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "File path is empty.");
                return;
            }

            if (!File.Exists(filePath))
            {
                DA.SetData(4, $"File not found: {filePath}");
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"File not found: {filePath}");
                return;
            }

            try
            {
                Encoding enc = ResolveEncoding(encodingId);
                char sep = delimiter[0];
                var allRows = new List<string[]>();
                var headers = new List<string>();

                using (var reader = new StreamReader(filePath, enc))
                {
                    bool firstLine = true;
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        string[] cells = ParseCsvLine(line, sep);

                        if (firstLine && hasHeaders)
                        {
                            foreach (var h in cells) headers.Add(h.Trim());
                            firstLine = false;
                            continue;
                        }

                        firstLine = false;
                        allRows.Add(cells);
                    }
                }

                if (allRows.Count == 0)
                {
                    DA.SetData(4, "No data rows found.");
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No data rows found.");
                    return;
                }

                int colCount = 0;
                foreach (var row in allRows)
                    if (row.Length > colCount) colCount = row.Length;

                while (headers.Count < colCount)
                    headers.Add($"Column{headers.Count + 1}");

                // Build tree — branch {col} = all row values for that column
                var tree = new GH_Structure<GH_String>();

                for (int col = 0; col < colCount; col++)
                {
                    var path = new GH_Path(col);
                    foreach (var row in allRows)
                        tree.Append(new GH_String(col < row.Length ? row[col] : ""), path);
                }

                DA.SetDataTree(0, tree);
                DA.SetDataList(1, headers);
                DA.SetData(2, allRows.Count);
                DA.SetData(3, colCount);
                DA.SetData(4, "");
            }
            catch (Exception ex)
            {
                DA.SetData(4, ex.Message);
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
            }
        }

        /// Parses a single CSV line respecting quoted fields containing the delimiter or newlines.
        private static string[] ParseCsvLine(string line, char sep)
        {
            var cells = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"') { current.Append('"'); i++; }
                        else inQuotes = false;
                    }
                    else current.Append(c);
                }
                else
                {
                    if (c == '"') inQuotes = true;
                    else if (c == sep) { cells.Add(current.ToString()); current.Clear(); }
                    else current.Append(c);
                }
            }

            cells.Add(current.ToString());
            return cells.ToArray();
        }

        private static Encoding ResolveEncoding(string id)
        {
            switch ((id ?? "UTF8").Trim().ToUpperInvariant())
            {
                case "ASCII": return Encoding.ASCII;
                case "UNICODE": return Encoding.Unicode;
                case "LATIN1":
                case "ISO-8859-1": return Encoding.GetEncoding(28591);
                default: return Encoding.UTF8;
            }
        }

        protected override Bitmap Icon => Properties.Resources.NC_ReadCsv;
        public override Guid ComponentGuid => new Guid("24A3EC97-30CF-4751-B183-60087EE4960C");
    }
}