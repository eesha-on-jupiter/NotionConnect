using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Drawing;

namespace NotionConnect.Presets
{
    public class HeadingStylePresetComponent : GH_ValueList
    {
        public HeadingStylePresetComponent()
        {
            Category = "NotionConnect";
            SubCategory = "Presets";
            Name = "Heading Style";
            NickName = "Heading Style";
            Description = "Notion heading level — H1, H2, or H3.";

            ListItems.Clear();
            ListItems.Add(new GH_ValueListItem("H1 — Large", "\"heading_1\""));
            ListItems.Add(new GH_ValueListItem("H2 — Medium", "\"heading_2\""));
            ListItems.Add(new GH_ValueListItem("H3 — Small", "\"heading_3\""));
        }

        protected override Bitmap Icon => Properties.Resources.NC_HeadingPreset;
        public override Guid ComponentGuid => new Guid("671D2250-CB1C-498C-9001-3B95BC7F0348");
    }
}