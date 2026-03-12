using Newtonsoft.Json.Linq;
using System;
using System.Drawing;

namespace NotionConnect.Docs
{
    /// <summary>
    /// Doc Heading — fully expanded, no toggle.
    /// Outputs a heading block followed by all content blocks flat.
    /// Style input: 1 = H1, 2 = H2, 3 = H3 (default 2).
    ///
    /// Heading (component name)
    /// Image
    /// Heading3 "Inputs"  + table
    /// Heading3 "Outputs" + table
    /// </summary>
    public class DocHeadingComponent : DocComponent
    {
        public DocHeadingComponent()
          : base("Doc Heading", "DocHeading",
              "Documents components as a heading followed by content expanded flat below. Style: 1=H1, 2=H2, 3=H3.")
        { }

        protected override string WrapComponent(string name, JArray children, int style)
        {
            var bundle = new JArray();
            bundle.Add(JToken.Parse(HeadingJson(name, style)));
            foreach (var child in children)
                bundle.Add(child);
            return new JObject { ["_bundle"] = bundle }
                .ToString(Newtonsoft.Json.Formatting.None);
        }

        protected override Bitmap Icon => Properties.Resources.NC_DocHeading;
        public override Guid ComponentGuid => new Guid("5AE0AD89-92BE-4B87-AAAC-BAFE6233414E");
    }
}