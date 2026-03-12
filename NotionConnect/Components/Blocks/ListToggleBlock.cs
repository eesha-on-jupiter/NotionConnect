using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace NotionConnect
{
    public class ListToggleBlockComponent : GH_Component
    {
        public ListToggleBlockComponent()
          : base("List Toggle Block", "List Toggle",
              "Creates a Notion toggle block. One toggle per branch of children.",
              "NotionConnect", "Blocks")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "T", "Toggle title texts — one per toggle.", GH_ParamAccess.list, "Toggle");
            pManager.AddTextParameter("Children BlockJson", "B", "Tree of child block JSONs. Each branch = children for one toggle (by index).", GH_ParamAccess.tree);
            pManager.AddTextParameter("Color", "C", "Notion color (default, blue, red, etc.).", GH_ParamAccess.item, "default");
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BlockJson", "B", "Toggle block JSONs — one per toggle.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var texts = new List<string>();
            string color = "default";
            GH_Structure<GH_String> childrenTree;

            if (!DA.GetDataList(0, texts)) return;
            DA.GetData(2, ref color);
            DA.GetDataTree(1, out childrenTree);

            if (string.IsNullOrWhiteSpace(color)) color = "default";

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
                    outputJsons.Add(BlockBuilders.ToggleJson(text, color, children));
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

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_ListToggleBlock;
        public override Guid ComponentGuid => new Guid("7ACC6162-3F02-4497-964A-BE47267B4577");
    }
}