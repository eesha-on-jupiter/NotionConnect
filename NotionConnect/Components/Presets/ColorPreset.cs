using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NotionConnect.Presets
{
    public class ColorPresetComponent : GH_ValueList
    {
        public ColorPresetComponent()
        {
            Category = "NotionConnect";
            SubCategory = "Presets";
            Name = "Color Preset";
            NickName = "Color";
            Description = "Notion block and text color values.";

            ListItems.Clear();
            foreach (var kvp in Items)
                ListItems.Add(new GH_ValueListItem(kvp.Key, $"\"{kvp.Value}\""));
        }

        private static readonly Dictionary<string, string> Items = new Dictionary<string, string>
        {
            { "Default",         "default"          },
            { "Gray",            "gray"             },
            { "Brown",           "brown"            },
            { "Orange",          "orange"           },
            { "Yellow",          "yellow"           },
            { "Green",           "green"            },
            { "Blue",            "blue"             },
            { "Purple",          "purple"           },
            { "Pink",            "pink"             },
            { "Red",             "red"              },
            { "Gray Background",   "gray_background"   },
            { "Brown Background",  "brown_background"  },
            { "Orange Background", "orange_background" },
            { "Yellow Background", "yellow_background" },
            { "Green Background",  "green_background"  },
            { "Blue Background",   "blue_background"   },
            { "Purple Background", "purple_background" },
            { "Pink Background",   "pink_background"   },
            { "Red Background",    "red_background"    },
        };

        protected override Bitmap Icon => Properties.Resources.NC_ColorPreset;
        public override Guid ComponentGuid => new Guid("F49DE69B-EC82-4140-94A3-A8A9928E159B");
    }
}