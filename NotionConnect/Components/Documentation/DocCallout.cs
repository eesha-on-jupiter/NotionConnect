using Newtonsoft.Json.Linq;
using System;
using System.Drawing;

namespace NotionConnect.Docs
{
    /// <summary>
    /// Doc Callout — each component in a callout box with an emoji icon.
    /// Style input is ignored.
    ///
    /// 📦 Callout (component name)
    ///   ├── Image
    ///   ├── Heading3 "Inputs"  + table
    ///   └── Heading3 "Outputs" + table
    /// </summary>
    public class DocCalloutComponent : DocComponent
    {
        public DocCalloutComponent()
          : base("Doc Callout", "Doc Callout",
              "Documents components as Notion callout blocks. Style input is ignored.")
        { }

        protected override string WrapComponent(string name, JArray children, int style)
        {
            return new JObject
            {
                ["object"] = "block",
                ["type"] = "callout",
                ["callout"] = new JObject
                {
                    ["rich_text"] = DocBlockBuilders.RichTextArray(name),
                    ["icon"] = new JObject { ["type"] = "emoji", ["emoji"] = "📦" },
                    ["color"] = "gray_background",
                    ["children"] = children
                }
            }.ToString(Newtonsoft.Json.Formatting.None);
        }

        protected override Bitmap Icon => Properties.Resources.NC_DocCallout;
        public override Guid ComponentGuid => new Guid("7800C916-BF06-43CA-88CD-8F1224499DA2");
    }
}