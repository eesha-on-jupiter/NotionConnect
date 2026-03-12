using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using SDImage = System.Drawing.Bitmap;
using SDFormat = System.Drawing.Imaging.ImageFormat;

namespace NotionConnect
{
    public class ImageBlockComponent : GH_Component
    {
        public ImageBlockComponent()
          : base("Image Block", "Image",
              "Creates a Notion image block. Accepts URLs, local file paths, or Bitmaps.",
              "NotionConnect", "Blocks")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Notion internal integration token (required for paths and bitmaps).", GH_ParamAccess.item);
            pManager.AddTextParameter("URLs", "U", "External image URLs — referenced directly, no upload.", GH_ParamAccess.list);
            pManager.AddTextParameter("Paths", "P", "Local file paths — uploaded automatically.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Bitmaps", "I", "Bitmap objects — uploaded automatically.", GH_ParamAccess.list);
            pManager.AddTextParameter("Caption", "C", "Optional caption — one per image or a single value.", GH_ParamAccess.list, "");
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BlockJson", "B", "Image block JSONs — one per image.", GH_ParamAccess.list);
            pManager.AddTextParameter("Error", "E", "Errors per image (empty if OK).", GH_ParamAccess.list);
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
            var bitmaps = new List<GH_ObjectWrapper>();
            var captions = new List<string>();

            DA.GetData(0, ref token);
            DA.GetDataList(1, urls);
            DA.GetDataList(2, paths);
            DA.GetDataList(3, bitmaps);
            DA.GetDataList(4, captions);

            token = token?.Trim();

            if (urls.Count == 0 && paths.Count == 0 && bitmaps.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Connect URLs, Paths, or Bitmaps.");
                return;
            }

            if ((paths.Count > 0 || bitmaps.Count > 0) && string.IsNullOrWhiteSpace(token))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Token is required for paths and bitmaps.");
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
                blockJsons.Add(BlockBuilders.ImageBlockJson(url, null, caption));
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
                    string mime = GetMime(System.IO.Path.GetExtension(path));
                    byte[] bytes = System.IO.File.ReadAllBytes(path);
                    var res = client.UploadFileAsync(System.IO.Path.GetFileName(path), bytes, mime).GetAwaiter().GetResult();
                    if (!res.Item1) { blockJsons.Add(""); errors.Add(res.Item3); continue; }
                    blockJsons.Add(BlockBuilders.ImageBlockJson(null, res.Item2, caption));
                    errors.Add("");
                }
                catch (Exception ex) { blockJsons.Add(""); errors.Add(ex.Message); }
            }

            for (int i = 0; i < bitmaps.Count; i++)
            {
                SDImage bmp = bitmaps[i]?.Value as SDImage;
                int capIdx = urls.Count + paths.Count + i;
                string caption = capIdx < capCount ? captions[capIdx] ?? "" : "";
                if (bmp == null) { blockJsons.Add(""); errors.Add($"Bitmap {i} is null."); continue; }

                try
                {
                    byte[] bytes = ToPng(bmp);
                    var res = client.UploadFileAsync($"image_{i}.png", bytes, "image/png").GetAwaiter().GetResult();
                    if (!res.Item1) { blockJsons.Add(""); errors.Add(res.Item3); continue; }
                    blockJsons.Add(BlockBuilders.ImageBlockJson(null, res.Item2, caption));
                    errors.Add("");
                }
                catch (Exception ex) { blockJsons.Add(""); errors.Add(ex.Message); }
            }

            DA.SetDataList(0, blockJsons);
            DA.SetDataList(1, errors);
        }

        private static byte[] ToPng(SDImage bmp)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                bmp.Save(ms, SDFormat.Png);
                return ms.ToArray();
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
                default: return "image/png";
            }
        }

        protected override SDImage Icon => Properties.Resources.NC_ImageBlock;
        public override Guid ComponentGuid => new Guid("A1B2C3D4-1111-2222-3333-000000000001");
    }
}