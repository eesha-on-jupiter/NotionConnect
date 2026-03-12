using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace NotionConnect
{
    public class AppendBlocksComponent : ButtonComponent
    {
        public AppendBlocksComponent()
          : base("Append Blocks", "AppendBlocks",
              "Appends blocks to a Notion page without erasing existing content. Press the button to send.",
              "NotionConnect", "Pages")
        { }

        public override string ButtonLabel => "Append";

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Notion internal integration token.", GH_ParamAccess.item);
            pManager.AddTextParameter("Page ID", "PID", "Target Notion page ID.", GH_ParamAccess.item);
            pManager.AddTextParameter("BlockJson", "B", "Block JSON strings — accepts any tree structure, all blocks are flattened in order.", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("OK", "OK", "True if request succeeded.", GH_ParamAccess.item);
            pManager.AddTextParameter("Error", "E", "Error message, if any.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string token = "";
            string pageId = "";
            GH_Structure<GH_String> blockTree;

            if (!DA.GetData(0, ref token)) { DA.SetData(0, false); DA.SetData(1, "Token missing."); return; }
            if (!DA.GetData(1, ref pageId)) { DA.SetData(0, false); DA.SetData(1, "Page ID missing."); return; }
            if (!DA.GetDataTree(2, out blockTree)) { DA.SetData(0, false); DA.SetData(1, "No block JSONs provided."); return; }
            if (string.IsNullOrWhiteSpace(token)) { DA.SetData(0, false); DA.SetData(1, "Token is empty."); return; }
            if (string.IsNullOrWhiteSpace(pageId)) { DA.SetData(0, false); DA.SetData(1, "Page ID is empty."); return; }

            if (!IsTriggered) return;

            var allJsons = new List<string>();
            foreach (var branch in blockTree.Branches)
                foreach (var gh in branch)
                    if (gh != null && !string.IsNullOrWhiteSpace(gh.Value))
                        allJsons.Add(gh.Value);

            if (allJsons.Count == 0) { DA.SetData(0, false); DA.SetData(1, "All block JSONs are empty."); return; }

            JArray children = new JArray();
            try
            {
                for (int i = 0; i < allJsons.Count; i++)
                {
                    JToken tok = JToken.Parse(allJsons[i]);
                    if (tok.Type != JTokenType.Object)
                        throw new Exception($"BlockJson[{i}] is not a JSON object.");
                    children.Add(tok);
                }
            }
            catch (Exception ex)
            {
                DA.SetData(0, false);
                DA.SetData(1, "Invalid BlockJson:\n" + ex.Message);
                return;
            }

            try
            {
                var client = new NotionClient(token);
                var result = client.AppendBlocksAsync(pageId, children).GetAwaiter().GetResult();

                DA.SetData(0, result.Item1);
                DA.SetData(1, result.Item3);
            }
            catch (Exception ex)
            {
                DA.SetData(0, false);
                DA.SetData(1, ex.ToString());
            }
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_AppendBlock;
        public override Guid ComponentGuid => new Guid("61DC7305-3722-42DD-8079-7B4AAA126317");
    }
}