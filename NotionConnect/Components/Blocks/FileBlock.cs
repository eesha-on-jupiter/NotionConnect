using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace NotionConnect
{
    public class FileBlockComponent : GH_Component
    {
        public FileBlockComponent()
          : base("File Block", "File",
              "Creates a Notion file block. Accepts external URLs or local file paths.",
              "NotionConnect", "Blocks")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Notion internal integration token (required for local paths).", GH_ParamAccess.item);
            pManager.AddTextParameter("URLs", "U", "External file URLs — referenced directly.", GH_ParamAccess.list);
            pManager.AddTextParameter("Paths", "P", "Local file paths — uploaded automatically.", GH_ParamAccess.list);
            pManager.AddTextParameter("Caption", "C", "Optional caption text.", GH_ParamAccess.list, "");
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BlockJson", "B", "File block JSONs — one per file.", GH_ParamAccess.list);
            pManager.AddTextParameter("Error", "E", "Errors per file (empty if OK).", GH_ParamAccess.list);
        }

        protected override void BeforeSolveInstance()
        {
            base.BeforeSolveInstance();
            Params.Output[0].DataMapping = Grasshopper.Kernel.GH_DataMapping.Flatten;
            Params.Output[1].DataMapping = Grasshopper.Kernel.GH_DataMapping.Flatten;
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string token = null;
            var urls = new List<string>();
            var paths = new List<string>();
            var captions = new List<string>();

            DA.GetData(0, ref token);
            DA.GetDataList(1, urls);
            DA.GetDataList(2, paths);
            DA.GetDataList(3, captions);

            token = token?.Trim();

            if (urls.Count == 0 && paths.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Connect at least one of: URLs or Paths.");
                return;
            }

            if (paths.Count > 0 && string.IsNullOrWhiteSpace(token))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Token is required for local paths.");
                return;
            }

            var blockJsons = new List<string>();
            var errors = new List<string>();
            int capCount = captions.Count;
            NotionClient client = string.IsNullOrWhiteSpace(token) ? null : new NotionClient(token);

            for (int i = 0; i < urls.Count; i++)
            {
                string url = urls[i]?.Trim().Trim('"', '\'').Trim() ?? "";
                string caption = i < capCount ? captions[i] ?? "" : "";
                if (string.IsNullOrWhiteSpace(url)) { blockJsons.Add(""); errors.Add("Empty URL."); continue; }
                blockJsons.Add(BlockBuilders.FileBlockJson(url, null, caption));
                errors.Add("");
            }

            for (int i = 0; i < paths.Count; i++)
            {
                string path = paths[i]?.Trim().Trim('"', '\'').Trim() ?? "";
                string caption = (urls.Count + i) < capCount ? captions[urls.Count + i] ?? "" : "";
                if (string.IsNullOrWhiteSpace(path)) { blockJsons.Add(""); errors.Add("Empty path."); continue; }
                if (!System.IO.File.Exists(path)) { blockJsons.Add(""); errors.Add($"File not found: {path}"); continue; }

                try
                {
                    string mime = GetMime(Path.GetExtension(path));
                    byte[] bytes = System.IO.File.ReadAllBytes(path);
                    var res = client.UploadFileAsync(Path.GetFileName(path), bytes, mime).GetAwaiter().GetResult();
                    if (!res.Item1) { blockJsons.Add(""); errors.Add(res.Item3); continue; }
                    blockJsons.Add(BlockBuilders.FileBlockJson(null, res.Item2, caption));
                    errors.Add("");
                }
                catch (Exception ex) { blockJsons.Add(""); errors.Add(ex.Message); }
            }

            DA.SetDataList(0, blockJsons);
            DA.SetDataList(1, errors);
        }

        private static string GetMime(string ext)
        {
            switch (ext?.ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".png": return "image/png";
                case ".pdf": return "application/pdf";
                case ".gif": return "image/gif";
                case ".webp": return "image/webp";
                default: return "application/octet-stream";
            }
        }

        protected override Bitmap Icon => Properties.Resources.NC_FileBlock;
        public override Guid ComponentGuid => new Guid("A1B2C3D4-1111-2222-3333-000000000002");
    }
}