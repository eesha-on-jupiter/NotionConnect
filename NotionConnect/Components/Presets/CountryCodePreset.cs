using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NotionConnect.Presets
{
    public class CountryCodePresetComponent : GH_ValueList
    {
        public CountryCodePresetComponent()
        {
            Category = "NotionConnect";
            SubCategory = "Presets";
            Name = "Country Code";
            NickName = "Country Code";
            Description = "Common country dial codes for phone number properties.";

            ListItems.Clear();
            foreach (var kvp in Items)
                ListItems.Add(new GH_ValueListItem(kvp.Key, $"\"{kvp.Value}\""));
        }

        private static readonly Dictionary<string, string> Items = new Dictionary<string, string>
        {
            { "Australia    +61",  "+61"  },
            { "Brazil       +55",  "+55"  },
            { "Canada       +1",   "+1"   },
            { "China        +86",  "+86"  },
            { "France       +33",  "+33"  },
            { "Germany      +49",  "+49"  },
            { "India        +91",  "+91"  },
            { "Italy        +39",  "+39"  },
            { "Japan        +81",  "+81"  },
            { "Mexico       +52",  "+52"  },
            { "Netherlands  +31",  "+31"  },
            { "New Zealand  +64",  "+64"  },
            { "Norway       +47",  "+47"  },
            { "Portugal     +351", "+351" },
            { "Russia       +7",   "+7"   },
            { "Saudi Arabia +966", "+966" },
            { "Singapore    +65",  "+65"  },
            { "South Africa +27",  "+27"  },
            { "South Korea  +82",  "+82"  },
            { "Spain        +34",  "+34"  },
            { "Sweden       +46",  "+46"  },
            { "Switzerland  +41",  "+41"  },
            { "UAE          +971", "+971" },
            { "UK           +44",  "+44"  },
            { "USA          +1",   "+1"   },
        };

        protected override Bitmap Icon => Properties.Resources.NC_CountryCodePreset;
        public override Guid ComponentGuid => new Guid("7C804F36-6A61-42A9-83B1-B3C67436A2BA");
    }
}