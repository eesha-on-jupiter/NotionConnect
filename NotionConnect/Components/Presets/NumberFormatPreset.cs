using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NotionConnect.Presets
{
    public class NumberFormatPresetComponent : GH_ValueList
    {
        public NumberFormatPresetComponent()
        {
            Category = "NotionConnect";
            SubCategory = "Presets";
            Name = "Number Format";
            NickName = "Number Format";
            Description = "Notion number property display format.";

            ListItems.Clear();
            foreach (var kvp in Items)
                ListItems.Add(new GH_ValueListItem(kvp.Key, $"\"{kvp.Value}\""));
        }

        private static readonly Dictionary<string, string> Items = new Dictionary<string, string>
        {
            { "Number",               "number"                },
            { "Number with commas",   "number_with_commas"    },
            { "Percent",              "percent"               },
            { "Dollar",               "dollar"                },
            { "Canadian Dollar",      "canadian_dollar"       },
            { "Singapore Dollar",     "singapore_dollar"      },
            { "Euro",                 "euro"                  },
            { "Pound",                "pound"                 },
            { "Yen",                  "yen"                   },
            { "Ruble",                "ruble"                 },
            { "Rupee",                "rupee"                 },
            { "Won",                  "won"                   },
            { "Yuan",                 "yuan"                  },
            { "Real",                 "real"                  },
            { "Lira",                 "lira"                  },
            { "Rupiah",               "rupiah"                },
            { "Franc",                "franc"                 },
            { "Hong Kong Dollar",     "hong_kong_dollar"      },
            { "New Zealand Dollar",   "new_zealand_dollar"    },
            { "Krona",                "krona"                 },
            { "Norwegian Krone",      "norwegian_krone"       },
            { "Mexican Peso",         "mexican_peso"          },
            { "Rand",                 "rand"                  },
            { "New Taiwan Dollar",    "new_taiwan_dollar"     },
            { "Danish Krone",         "danish_krone"          },
            { "Zloty",                "zloty"                 },
            { "Baht",                 "baht"                  },
            { "Forint",               "forint"                },
            { "Koruna",               "koruna"                },
            { "Shekel",               "shekel"                },
            { "Chilean Peso",         "chilean_peso"          },
            { "Philippine Peso",      "philippine_peso"       },
            { "Dirham",               "dirham"                },
            { "Colombian Peso",       "colombian_peso"        },
            { "Riyal",                "riyal"                 },
            { "Ringgit",              "ringgit"               },
            { "Leu",                  "leu"                   },
            { "Argentine Peso",       "argentine_peso"        },
            { "Uruguayan Peso",       "uruguayan_peso"        },
            { "Peruvian Sol",         "peruvian_sol"          },
        };

        protected override Bitmap Icon => Properties.Resources.NC_NumberFormatPreset;
        public override Guid ComponentGuid => new Guid("2286120B-C8E4-47BB-A82C-A7361C5731C1");
    }
}