using Newtonsoft.Json.Linq;
using System;
using System.Drawing;

namespace NotionConnect.Docs
{
    /// <summary>
    /// Doc Divider — content blocks only (no heading), closed with a divider.
    /// Useful when the section heading is handled separately, or for quick
    /// visual separation between documented components.
    ///
    /// Layout:
    ///   Image
    ///   Heading3 "Inputs"  + table
    ///   Heading3 "Outputs" + table
    ///   Divider
    /// </summary>
    public class DocDividerComponent : DocComponent
    {
        public DocDividerComponent()
          : base("Doc Divider", "Doc Divider",
              "Documents a component as flat content blocks closed with a divider. No section heading is added.")
        { }

        protected override string WrapComponent(string name, JArray children, int style)
        {
            var bundle = new JArray();
            foreach (var child in children)
                bundle.Add(child);
            bundle.Add(JToken.Parse(BlockBuilders.DividerJson()));
            return new JObject { ["_bundle"] = bundle }
                .ToString(Newtonsoft.Json.Formatting.None);
        }

        protected override Bitmap Icon => Properties.Resources.NC_DocDivide;
        public override Guid ComponentGuid => new Guid("6C3F2A3B-A722-4997-B13A-CE0963583B93");
    }
}