using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NotionConnect
{
    public class EmbedBlockComponent : GH_Component
    {
        public EmbedBlockComponent()
          : base("Embed Block", "Embed",
              "Creates a Notion embed or video block from a URL.",
              "NotionConnect", "Blocks")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("URLs", "U", "URLs to embed or play.", GH_ParamAccess.list);
            pManager.AddTextParameter("Type", "T", "Block type: 'embed' (Figma, maps) or 'video' (YouTube, Vimeo).", GH_ParamAccess.item, "embed");
            pManager.AddTextParameter("Caption", "C", "Optional caption text.", GH_ParamAccess.list, "");
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BlockJson", "B", "Embed block JSONs — one per URL.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var urls = new List<string>();
            string type = "embed";
            var captions = new List<string>();

            if (!DA.GetDataList(0, urls)) return;
            DA.GetData(1, ref type);
            DA.GetDataList(2, captions);

            type = type?.Trim().ToLowerInvariant();
            if (type != "video") type = "embed";

            var output = new List<string>();
            for (int i = 0; i < urls.Count; i++)
            {
                string url = urls[i]?.Trim().Trim('"', '\'').Trim() ?? "";
                string caption = i < captions.Count ? captions[i] ?? "" : "";
                if (string.IsNullOrWhiteSpace(url)) { output.Add(""); continue; }
                output.Add(type == "video"
                    ? BlockBuilders.VideoBlockJson(url, caption)
                    : BlockBuilders.EmbedJson(url, caption));
            }

            DA.SetDataList(0, output);
        }

        protected override Bitmap Icon => Properties.Resources.NC_EmbedBlock;
        public override Guid ComponentGuid => new Guid("A1B2C3D4-1111-2222-3333-000000000003");
    }
}