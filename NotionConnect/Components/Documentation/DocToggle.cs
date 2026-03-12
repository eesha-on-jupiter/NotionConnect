using Newtonsoft.Json.Linq;
using System;
using System.Drawing;

namespace NotionConnect.Docs
{
    /// <summary>
    /// Doc Toggle — H1/H2/H3 collapsible toggle heading.
    /// Style input: 1 = H1, 2 = H2, 3 = H3 (default 2).
    ///
    /// Toggle H2 (component name)
    ///   ├── Image
    ///   ├── Heading3 "Inputs"  + table
    ///   └── Heading3 "Outputs" + table
    /// </summary>
    public class DocToggleComponent : DocComponent
    {
        public DocToggleComponent()
          : base("Doc Toggle", "DocToggle",
              "Documents components as collapsible heading toggles. Style: 1=H1, 2=H2, 3=H3.")
        { }

        protected override string WrapComponent(string name, JArray children, int style)
        {
            string type = style == 1 ? "heading_1" : style == 2 ? "heading_2" : "heading_3";
            return new JObject
            {
                ["object"] = "block",
                ["type"] = type,
                [type] = new JObject
                {
                    ["rich_text"] = DocBlockBuilders.RichTextArray(name),
                    ["color"] = "default",
                    ["is_toggleable"] = true,
                    ["children"] = children
                }
            }.ToString(Newtonsoft.Json.Formatting.None);
        }

        protected override Bitmap Icon => Properties.Resources.NC_DocToggle;
        public override Guid ComponentGuid => new Guid("7800C913-BF06-43CA-88CD-8F1224499DA2");
    }
}