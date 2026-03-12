using Newtonsoft.Json.Linq;
using System;
using System.Drawing;

namespace NotionConnect.Docs
{
    /// <summary>
    /// Doc Toggle Plain — standard Notion toggle block (no heading style).
    /// Style input is ignored.
    ///
    /// Toggle (component name)
    ///   ├── Image
    ///   ├── Heading3 "Inputs"  + table
    ///   └── Heading3 "Outputs" + table
    /// </summary>
    public class DocTogglePlainComponent : DocComponent
    {
        public DocTogglePlainComponent()
          : base("Doc Toggle Plain", "DocTogglePlain",
              "Documents components as plain Notion toggle blocks. Style input is ignored.")
        { }

        protected override string WrapComponent(string name, JArray children, int style)
        {
            return BlockBuilders.ToggleJson(name, "default", children);
        }

        protected override Bitmap Icon => Properties.Resources.NC_DocPlainToggle;
        public override Guid ComponentGuid => new Guid("4F58D1F6-3586-4F84-9AD7-F307E47F2F9C");
    }
}