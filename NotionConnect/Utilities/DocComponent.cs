using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NotionConnect.Docs
{
    public abstract class DocComponent : GH_Component
    {
        protected DocComponent(string name, string nick, string desc)
          : base(name, nick, desc, "NotionConnect", "Docs") { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Notion internal integration token.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Component", "C", "Wire any output from the components to document.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Style", "S", "Heading style: 1 = H1, 2 = H2, 3 = H3.", GH_ParamAccess.item, 2);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BlockJson", "B", "Block JSONs — wire into Append Blocks or Rewrite Blocks.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Error", "E", "Status or error message.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Subclasses implement this to wrap children in their chosen block style.
        /// Return a single block JSON string, OR {"_bundle":[...]} for multiple flat blocks.
        /// </summary>
        protected abstract string WrapComponent(string name, JArray children, int style);

        private bool _solved = false;

        protected override void BeforeSolveInstance()
        {
            _solved = false;
            base.BeforeSolveInstance();
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (_solved) return;
            _solved = true;

            string token = null;
            int style = 2;

            if (!DA.GetData(0, ref token)) return;
            DA.GetData(2, ref style);

            token = token?.Trim();
            if (string.IsNullOrWhiteSpace(token)) { DA.SetData(1, "Token required."); return; }
            if (style < 1 || style > 3) style = 2;

            var comps = new List<IGH_Component>();
            foreach (var src in Params.Input[1].Sources)
            {
                var comp = src?.Attributes?.GetTopLevel?.DocObject as IGH_Component
                        ?? src?.Attributes?.DocObject as IGH_Component;
                if (comp != null && !comps.Contains(comp))
                    comps.Add(comp);
            }

            if (comps.Count == 0) { DA.SetData(1, "No components resolved."); return; }

            var canvas = Instances.ActiveCanvas;
            var client = new NotionClient(token);
            var allBlocks = new List<string>();
            var statuses = new List<string>();

            foreach (var comp in comps)
            {
                try
                {
                    // ---- Screenshot ----
                    string imageFileId = null;
                    Bitmap bmp = null;

                    Rhino.RhinoApp.InvokeOnUiThread((Action)(() =>
                    {
                        bmp = docCapture.CaptureComponent(canvas, comp);
                    }));

                    if (bmp != null)
                    {
                        byte[] png = docCapture.ToPng(bmp);
                        var up = client.UploadFileAsync(
                            $"{comp.Name.Replace(" ", "_")}_doc.png", png, "image/png")
                            .GetAwaiter().GetResult();
                        if (up.Item1) imageFileId = up.Item2;
                    }

                    // ---- Build inner blocks ----
                    var children = new JArray();
                    var nickNames = GetNickNames(comp);

                    if (!string.IsNullOrWhiteSpace(imageFileId))
                        children.Add(JToken.Parse(BlockBuilders.ImageBlockJson(null, imageFileId, comp.Name)));

                    children.Add(JToken.Parse(DocBlockBuilders.MetaLineJson(comp.Description, comp.Category, comp.SubCategory)));

                    children.Add(JToken.Parse(BlockBuilders.Heading3Json("Inputs")));
                    children.Add(JToken.Parse(DocBlockBuilders.BuildParamTable(comp.Params.Input, nickNames)));
                    children.Add(JToken.Parse(BlockBuilders.Heading3Json("Outputs")));
                    children.Add(JToken.Parse(DocBlockBuilders.BuildParamTable(comp.Params.Output, nickNames)));

                    // ---- Wrap ----
                    string wrapped = WrapComponent(comp.Name, children, style);
                    var parsed = JObject.Parse(wrapped);

                    if (parsed["_bundle"] is JArray bundle)
                        foreach (var b in bundle)
                            allBlocks.Add(b.ToString(Newtonsoft.Json.Formatting.None));
                    else
                        allBlocks.Add(wrapped);

                    statuses.Add($"✓ {comp.Name}");
                }
                catch (Exception ex)
                {
                    statuses.Add($"✗ {comp.Name}: {ex.Message}");
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"{comp.Name}: {ex.Message}");
                }
            }

            var tree = new GH_Structure<GH_String>();
            var path = new GH_Path(0);
            foreach (var b in allBlocks)
                tree.Append(new GH_String(b), path);

            DA.SetDataTree(0, tree);
            DA.SetData(1, string.Join("\n", statuses));
        }

        protected static string HeadingJson(string text, int level)
            => BlockBuilders.HeadingJson(text, level);

        private static Dictionary<Guid, string> GetNickNames(IGH_Component comp)
        {
            var result = new Dictionary<Guid, string>();
            try
            {
                var fresh = Instances.ComponentServer.EmitObject(comp.ComponentGuid) as IGH_Component;
                if (fresh == null) return result;

                var li = comp.Params.Input; var fi = fresh.Params.Input;
                for (int i = 0; i < li.Count && i < fi.Count; i++)
                    result[li[i].InstanceGuid] = fi[i].NickName;

                var lo = comp.Params.Output; var fo = fresh.Params.Output;
                for (int i = 0; i < lo.Count && i < fo.Count; i++)
                    result[lo[i].InstanceGuid] = fo[i].NickName;
            }
            catch { }
            return result;
        }
    }
}