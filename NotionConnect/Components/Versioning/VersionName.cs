using Grasshopper.Kernel;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace NotionConnect
{
    public class VersionNameComponent : ButtonComponent
    {
        private bool _resetRequested = false;

        public override string ButtonLabel => "Reset";

        public VersionNameComponent()
          : base("Version Name", "Version Name",
              "Builds a version name from prefix, date, and number. Press reset to restart sequence from 0.",
              "NotionConnect", "Versioning")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Prefix", "P", "Version prefix, e.g. 'TowerStudy'.", GH_ParamAccess.item, "v");
            pManager.AddBooleanParameter("Include Date", "D", "Include today's date in the version name.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Include Number", "N", "Include an auto-incrementing number.", GH_ParamAccess.item, true);
            pManager.AddTextParameter("Date Format", "DF", "Date format string, e.g. 'yyyy-MM-dd'.", GH_ParamAccess.item, "yyyy-MM-dd");
            pManager.AddIntegerParameter("Padding", "PAD", "Digit count for version number e.g. 3 → '001'.", GH_ParamAccess.item, 3);
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Version Name", "VN", "Assembled version name — wire into Version Save.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Next Number", "NN", "Next version number that will be used.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string prefix = "v";
            bool includeDate = false;
            bool includeNumber = true;
            string dateFormat = "yyyy-MM-dd";
            int padding = 3;

            DA.GetData(0, ref prefix);
            DA.GetData(1, ref includeDate);
            DA.GetData(2, ref includeNumber);
            DA.GetData(3, ref dateFormat);
            DA.GetData(4, ref padding);

            prefix = string.IsNullOrWhiteSpace(prefix) ? "v" : prefix.Trim();
            dateFormat = string.IsNullOrWhiteSpace(dateFormat) ? "yyyy-MM-dd" : dateFormat.Trim();
            if (padding < 1) padding = 1;

            if (IsTriggered || _resetRequested)
            {
                SequenceStore.Write(prefix, 0);
                _resetRequested = false;
            }

            int currentN = SequenceStore.Read(prefix);
            string name = Build(prefix, includeDate, includeNumber, dateFormat, padding, currentN);

            DA.SetData(0, name);
            DA.SetData(1, currentN + 1);
        }

        /// <summary>
        /// Called by VersionSave after a successful push.
        /// Increments the sequence and schedules a recompute of this component.
        /// </summary>
        public void IncrementAndRefresh(string prefix)
        {
            SequenceStore.Write(prefix, SequenceStore.Read(prefix) + 1);

            Rhino.RhinoApp.InvokeOnUiThread((Action)(() =>
            {
                var timer = new System.Windows.Forms.Timer { Interval = 100 };
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    timer.Dispose();
                    ExpireSolution(true);
                };
                timer.Start();
            }));
        }

        public static string Build(string prefix, bool includeDate, bool includeNumber,
            string dateFormat, int padding, int currentN)
        {
            var parts = new System.Collections.Generic.List<string>();

            if (!string.IsNullOrWhiteSpace(prefix))
                parts.Add(prefix.Trim());

            if (includeDate)
                parts.Add(DateTime.Now.ToString(dateFormat, CultureInfo.InvariantCulture));

            if (includeNumber)
                parts.Add((currentN + 1).ToString($"D{padding}"));

            return string.Join("_", parts);
        }

        protected override Bitmap Icon => Properties.Resources.NC_VersionName;
        public override Guid ComponentGuid => new Guid("ABE742CE-BD76-4F63-B50B-D68B6F5CC311");
    }

    public static class SequenceStore
    {
        private static string GetPath(string prefix)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                prefix = prefix.Replace(c, '_');

            string dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NotionConnect", "sequences");

            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"{prefix}.json");
        }

        public static int Read(string prefix)
        {
            try
            {
                string path = GetPath(prefix);
                if (!File.Exists(path)) return 0;
                var json = JObject.Parse(File.ReadAllText(path));
                return json["n"]?.Value<int>() ?? 0;
            }
            catch { return 0; }
        }

        public static void Write(string prefix, int value)
        {
            try
            {
                File.WriteAllText(GetPath(prefix), new JObject { ["n"] = value }.ToString());
            }
            catch { }
        }
    }
}