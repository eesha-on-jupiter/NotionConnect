using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NotionConnect.Presets
{
    public class CodeLanguagePresetComponent : GH_ValueList
    {
        public CodeLanguagePresetComponent()
        {
            Category = "NotionConnect";
            SubCategory = "Presets";
            Name = "Code Language";
            NickName = "Code Lang";
            Description = "Notion code block language values.";

            ListItems.Clear();
            foreach (var kvp in Items)
                ListItems.Add(new GH_ValueListItem(kvp.Key, $"\"{kvp.Value}\""));
        }

        private static readonly Dictionary<string, string> Items = new Dictionary<string, string>
        {
            { "Plain Text",  "plain text"  },
            { "ABAP",        "abap"        },
            { "Arduino",     "arduino"     },
            { "Bash",        "bash"        },
            { "BASIC",       "basic"       },
            { "C",           "c"           },
            { "Clojure",     "clojure"     },
            { "CoffeeScript","coffeescript"},
            { "C++",         "c++"         },
            { "C#",          "c#"          },
            { "CSS",         "css"         },
            { "Dart",        "dart"        },
            { "Diff",        "diff"        },
            { "Docker",      "docker"      },
            { "Elixir",      "elixir"      },
            { "Elm",         "elm"         },
            { "Erlang",      "erlang"      },
            { "Flow",        "flow"        },
            { "Fortran",     "fortran"     },
            { "F#",          "f#"          },
            { "Gherkin",     "gherkin"     },
            { "GLSL",        "glsl"        },
            { "Go",          "go"          },
            { "GraphQL",     "graphql"     },
            { "Groovy",      "groovy"      },
            { "Haskell",     "haskell"     },
            { "HTML",        "html"        },
            { "Java",        "java"        },
            { "JavaScript",  "javascript"  },
            { "JSON",        "json"        },
            { "Julia",       "julia"       },
            { "Kotlin",      "kotlin"      },
            { "LaTeX",       "latex"       },
            { "Less",        "less"        },
            { "Lisp",        "lisp"        },
            { "LiveScript",  "livescript"  },
            { "Lua",         "lua"         },
            { "Makefile",    "makefile"    },
            { "Markdown",    "markdown"    },
            { "Markup",      "markup"      },
            { "MATLAB",      "matlab"      },
            { "Mermaid",     "mermaid"     },
            { "Nix",         "nix"         },
            { "Objective-C", "objective-c" },
            { "OCaml",       "ocaml"       },
            { "Pascal",      "pascal"      },
            { "Perl",        "perl"        },
            { "PHP",         "php"         },
            { "PowerShell",  "powershell"  },
            { "Prolog",      "prolog"      },
            { "Protobuf",    "protobuf"    },
            { "Python",      "python"      },
            { "R",           "r"           },
            { "Reason",      "reason"      },
            { "Ruby",        "ruby"        },
            { "Rust",        "rust"        },
            { "Sass",        "sass"        },
            { "Scala",       "scala"       },
            { "Scheme",      "scheme"      },
            { "Scss",        "scss"        },
            { "Shell",       "shell"       },
            { "SQL",         "sql"         },
            { "Swift",       "swift"       },
            { "TypeScript",  "typescript"  },
            { "VB.Net",      "vb.net"      },
            { "Verilog",     "verilog"     },
            { "VHDL",        "vhdl"        },
            { "Visual Basic","visual basic"},
            { "WebAssembly", "webassembly" },
            { "XML",         "xml"         },
            { "YAML",        "yaml"        },
        };

        protected override Bitmap Icon => Properties.Resources.NC_CodePreset;
        public override Guid ComponentGuid => new Guid("3158CCF5-9033-4CD9-80E7-5A17C273DD69");
    }
}