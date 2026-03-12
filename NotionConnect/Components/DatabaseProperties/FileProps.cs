using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;

namespace NotionConnect
{
    public class FilePropComponent : GH_Component
    {
        private static readonly HttpClient _downloader = new HttpClient();

        public FilePropComponent()
          : base("File Prop", "File",
              "Creates a Notion database File property. Uploads URLs, local file paths, or Bitmaps to Notion.",
              "NotionConnect", "DatabaseProperties")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Header", "H", "Property name (column header in Notion database).", GH_ParamAccess.item, "Files");
            pManager.AddTextParameter("Token", "T", "Notion internal integration token (required for uploads).", GH_ParamAccess.item);
            pManager.AddTextParameter("URLs", "U", "External file URLs — one per row.", GH_ParamAccess.list);
            pManager.AddTextParameter("Paths", "P", "Local file paths — uploaded automatically.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Bitmaps", "I", "Bitmap objects — uploaded automatically.", GH_ParamAccess.list);
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("HeadingJson", "HJ", "Property definition JSON for the database schema.", GH_ParamAccess.item);
            pManager.AddTextParameter("RowJson", "RJ", "Row value JSONs — one branch per row.", GH_ParamAccess.tree);
            pManager.AddTextParameter("FileIds", "IDs", "Uploaded Notion file IDs — one per row.", GH_ParamAccess.list);
            pManager.AddTextParameter("Error", "E", "Upload errors per row (empty if OK).", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "Files";
            string token = null;
            var urls = new List<string>();
            var paths = new List<string>();
            var bitmaps = new List<GH_ObjectWrapper>();

            DA.GetData(0, ref name);
            if (!DA.GetData(1, ref token)) return;
            DA.GetDataList(2, urls);
            DA.GetDataList(3, paths);
            DA.GetDataList(4, bitmaps);

            if (string.IsNullOrWhiteSpace(name)) name = "Unnamed";
            token = token?.Trim();

            if (string.IsNullOrWhiteSpace(token))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Token is required for file upload.");
                return;
            }

            if (urls.Count == 0 && paths.Count == 0 && bitmaps.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Connect at least one of: URLs, Paths, or Bitmaps.");
                DA.SetData(0, DatabasePropertyBuilders.Files(name));
                return;
            }

            var uploads = new List<(string fileName, byte[] bytes, string mime, string error)>();

            foreach (string rawUrl in urls)
            {
                string url = rawUrl?.Trim().Trim('"', '\'').Trim();
                if (string.IsNullOrWhiteSpace(url)) { uploads.Add((null, null, null, "Empty URL.")); continue; }
                try
                {
                    byte[] bytes = _downloader.GetByteArrayAsync(url).GetAwaiter().GetResult();
                    string mime = GuessUrlMime(url);
                    uploads.Add((GuessFileName(url, mime), bytes, mime, null));
                }
                catch (Exception ex) { uploads.Add((null, null, null, $"URL download failed: {ex.Message}")); }
            }

            foreach (string rawPath in paths)
            {
                string path = rawPath?.Trim().Trim('"', '\'').Trim();
                if (string.IsNullOrWhiteSpace(path)) { uploads.Add((null, null, null, "Empty path.")); continue; }
                if (!System.IO.File.Exists(path)) { uploads.Add((null, null, null, $"File not found: {path}")); continue; }
                try
                {
                    string mime = GetMime(Path.GetExtension(path));
                    byte[] bytes = System.IO.File.ReadAllBytes(path);
                    uploads.Add((Path.GetFileName(path), bytes, mime, null));
                }
                catch (Exception ex) { uploads.Add((null, null, null, $"File read failed: {ex.Message}")); }
            }

            for (int i = 0; i < bitmaps.Count; i++)
            {
                Bitmap bmp = bitmaps[i]?.Value as Bitmap;
                if (bmp == null) { uploads.Add((null, null, null, $"Bitmap {i} is null.")); continue; }
                try { uploads.Add(($"image_{i}.png", BitmapToPng(bmp), "image/png", null)); }
                catch (Exception ex) { uploads.Add((null, null, null, $"Bitmap {i} convert failed: {ex.Message}")); }
            }

            var client = new NotionClient(token);
            var fileIds = new List<string>();
            var errors = new List<string>();
            var tree = new Grasshopper.DataTree<string>();

            for (int i = 0; i < uploads.Count; i++)
            {
                var (fileName, bytes, mime, prepError) = uploads[i];

                if (prepError != null)
                {
                    fileIds.Add(""); errors.Add(prepError);
                    tree.Add(new JObject { ["files"] = new JArray() }.ToString(), new GH_Path(i));
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Row {i}: {prepError}");
                    continue;
                }

                if (bytes.Length > 5 * 1024 * 1024)
                {
                    string msg = $"Row {i}: {bytes.Length / 1024 / 1024}MB exceeds Notion 5MB limit.";
                    fileIds.Add(""); errors.Add(msg);
                    tree.Add(new JObject { ["files"] = new JArray() }.ToString(), new GH_Path(i));
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, msg);
                    continue;
                }

                var res = client.UploadFileAsync(fileName, bytes, mime).GetAwaiter().GetResult();
                if (!res.Item1)
                {
                    fileIds.Add(""); errors.Add(res.Item3);
                    tree.Add(new JObject { ["files"] = new JArray() }.ToString(), new GH_Path(i));
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Row {i} upload failed: {res.Item3}");
                    continue;
                }

                fileIds.Add(res.Item2);
                errors.Add("");
                tree.Add(new JObject
                {
                    ["files"] = new JArray
                    {
                        new JObject
                        {
                            ["type"]        = "file_upload",
                            ["name"]        = fileName,
                            ["file_upload"] = new JObject { ["id"] = res.Item2 }
                        }
                    }
                }.ToString(), new GH_Path(i));

                if (i < uploads.Count - 1)
                    System.Threading.Tasks.Task.Delay(350).GetAwaiter().GetResult();
            }

            DA.SetData(0, DatabasePropertyBuilders.Files(name));
            DA.SetDataTree(1, tree);
            DA.SetDataList(2, fileIds);
            DA.SetDataList(3, errors);
        }

        private static byte[] BitmapToPng(Bitmap bmp)
        {
            using (var ms = new MemoryStream()) { bmp.Save(ms, ImageFormat.Png); return ms.ToArray(); }
        }

        private static string GuessUrlMime(string url)
        {
            string l = url.ToLowerInvariant();
            if (l.Contains(".jpg") || l.Contains(".jpeg") || l.Contains("fm=jpg")) return "image/jpeg";
            if (l.Contains(".png")) return "image/png";
            if (l.Contains(".gif")) return "image/gif";
            if (l.Contains(".webp")) return "image/webp";
            if (l.Contains(".pdf")) return "application/pdf";
            return "image/jpeg";
        }

        private static string GuessFileName(string url, string mime)
        {
            try
            {
                string p = new Uri(url).AbsolutePath;
                string name = Path.GetFileName(p);
                if (!string.IsNullOrWhiteSpace(name) && name.Contains(".")) return name;
            }
            catch { }
            switch (mime)
            {
                case "image/jpeg": return "image.jpg";
                case "image/png": return "image.png";
                case "image/gif": return "image.gif";
                case "image/webp": return "image.webp";
                case "application/pdf": return "file.pdf";
                default: return "file";
            }
        }

        private static string GetMime(string ext)
        {
            switch (ext?.ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                case ".webp": return "image/webp";
                case ".pdf": return "application/pdf";
                default: return "application/octet-stream";
            }
        }

        protected override Bitmap Icon => Properties.Resources.NC_FileProp;
        public override Guid ComponentGuid => new Guid("F5A6B7C8-D9E0-1234-FABC-345678900005");
    }
}