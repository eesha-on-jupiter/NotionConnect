using Newtonsoft.Json.Linq;
using System;
using System.Drawing;

namespace NotionConnect.Docs
{
    /// <summary>
    /// Doc Heading Divider — heading block followed by content flat, then a divider at the end.
    /// Useful as a clear section separator in documentation pages.
    ///
    /// Layout:
    ///   Heading (component name)
    ///   Image
    ///   Heading3 "Inputs"  + table
    ///   Heading3 "Outputs" + table
    ///   Divider
    /// </summary>
    public class DocHeadingDividerComponent : DocComponent
    {
        public DocHeadingDividerComponent()
          : base("Doc Heading Divider", "Doc Heading Divider",
              "Documents a component as a heading with content expanded flat below, closed with a divider. Style: 1=H1, 2=H2, 3=H3.")
        { }

        protected override string WrapComponent(string name, JArray children, int style)
        {
            var bundle = new JArray();
            bundle.Add(JToken.Parse(HeadingJson(name, style)));
            foreach (var child in children)
                bundle.Add(child);
            bundle.Add(JToken.Parse(BlockBuilders.DividerJson()));
            return new JObject { ["_bundle"] = bundle }
                .ToString(Newtonsoft.Json.Formatting.None);
        }

        protected override Bitmap Icon => Properties.Resources.NC_DocHeadingDivide;
        public override Guid ComponentGuid => new Guid("0C8B0F10-0C64-4CD4-B0DE-A22314C3DF87");
    }
}