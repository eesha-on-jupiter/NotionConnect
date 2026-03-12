using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace NotionConnect
{
    public class ToggleHeadingBlockComponent : GH_Component
    {
        public ToggleHeadingBlockComponent()
          : base("Toggle Heading Block", "Toggle Heading",
              "Creates a collapsible Notion heading block. One toggle per branch of children.",
              "NotionConnect", "Blocks")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "T", "Heading texts — one per toggle.", GH_ParamAccess.list, "Section");
            pManager.AddTextParameter("Children BlockJson", "B", "Tree of child block JSONs. Each branch = children for one toggle (by index).", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Style", "S", "Heading level: 1 = H1, 2 = H2, 3 = H3.", GH_ParamAccess.item, 2);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BlockJson", "B", "Toggle heading block JSONs — one per toggle.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var texts = new List<string>();
            int style = 2;
            GH_Structure<GH_String> childrenTree;

            if (!DA.GetDataList(0, texts)) return;
            DA.GetData(2, ref style);
            DA.GetDataTree(1, out childrenTree);

            if (style < 1 || style > 3) style = 2;
            string type = style == 1 ? "heading_1" : style == 2 ? "heading_2" : "heading_3";

            var outputJsons = new List<string>();

            for (int i = 0; i < texts.Count; i++)
            {
                string text = texts[i] ?? "";
                JArray children = new JArray();

                if (childrenTree != null && i < childrenTree.Branches.Count)
                {
                    var branch = childrenTree.Branches[i];
                    for (int j = 0; j < branch.Count; j++)
                    {
                        string s = branch[j]?.Value;
                        if (string.IsNullOrWhiteSpace(s)) continue;

                        try
                        {
                            var tok = JToken.Parse(s);
                            if (tok.Type == JTokenType.Object)
                                children.Add(tok);
                            else
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                                    $"Toggle {i}, child {j}: not a JSON object — skipped.");
                        }
                        catch (Exception ex)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                                $"Toggle {i}, child {j}: invalid JSON — {ex.Message}");
                        }
                    }
                }

                try
                {
                    var inner = new JObject
                    {
                        ["rich_text"] = new JArray
                        {
                            new JObject
                            {
                                ["type"] = "text",
                                ["text"] = new JObject { ["content"] = text }
                            }
                        },
                        ["color"] = "default",
                        ["is_toggleable"] = true
                    };

                    if (children.Count > 0)
                        inner["children"] = children;

                    var block = new JObject
                    {
                        ["object"] = "block",
                        ["type"] = type,
                        [type] = inner
                    };

                    outputJsons.Add(block.ToString(Newtonsoft.Json.Formatting.None));
                }
                catch (Exception ex)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                        $"Toggle {i}: failed to build — {ex.Message}");
                    outputJsons.Add("");
                }
            }

            DA.SetDataList(0, outputJsons);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_ToggleHeadingBlock;
        public override Guid ComponentGuid => new Guid("33B73B1A-2280-46DC-8833-8C7BB8D155AF");
    }
}