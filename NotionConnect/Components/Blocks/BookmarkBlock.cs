using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace NotionConnect
{
    public class BookmarkBlockComponent : GH_Component
    {
        public BookmarkBlockComponent()
          : base("Bookmark Block", "Bookmark",
              "Creates a Notion bookmark block — a visual URL card.",
              "NotionConnect", "Blocks")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("URL", "U", "URL to bookmark.", GH_ParamAccess.list);
            pManager.AddTextParameter("Caption", "C", "Optional caption text.", GH_ParamAccess.list, "");
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BlockJson", "B", "Bookmark block JSONs — one per item.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var urls = new List<string>();
            var captions = new List<string>();

            if (!DA.GetDataList(0, urls)) return;
            DA.GetDataList(1, captions);

            var output = new List<string>();
            for (int i = 0; i < urls.Count; i++)
            {
                string url = urls[i]?.Trim() ?? "";
                string caption = i < captions.Count ? captions[i] ?? "" : "";
                output.Add(BlockBuilders.BookmarkJson(url, caption));
            }

            DA.SetDataList(0, output);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_BookmarkBlock;
        public override Guid ComponentGuid => new Guid("A1B2C3D4-1111-2222-3333-000000000004");
    }
}