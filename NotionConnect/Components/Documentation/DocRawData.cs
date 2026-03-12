using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NotionConnect.Docs
{
    /// <summary>
    /// Extracts raw metadata from wired components — no Notion calls, no screenshots.
    /// One item per component for flat lists, one branch per component for param trees.
    /// </summary>
    public class DocRawDataComponent : GH_Component
    {
        public DocRawDataComponent()
          : base("Doc Raw Data", "Raw Data",
              "Extracts raw metadata from wired components: names, nicknames, categories, descriptions, and full input/output param trees.",
              "NotionConnect", "Docs")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Component", "C", "Wire any output from the components to inspect.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // Flat lists — one item per component
            pManager.AddTextParameter("Name", "N", "Component names.", GH_ParamAccess.list);
            pManager.AddTextParameter("Nickname", "NK", "Component nicknames.", GH_ParamAccess.list);
            pManager.AddTextParameter("Category", "CAT", "Component categories.", GH_ParamAccess.list);
            pManager.AddTextParameter("SubCategory", "SUB", "Component subcategories.", GH_ParamAccess.list);
            pManager.AddTextParameter("Description", "D", "Component descriptions.", GH_ParamAccess.list);

            // Input param trees — one branch per component
            pManager.AddTextParameter("Input Names", "IN", "Input parameter names — one branch per component.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Input Nicknames", "INK", "Input parameter nicknames — one branch per component.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Input Descriptions", "ID", "Input parameter descriptions — one branch per component.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Input Types", "IT", "Input parameter types — one branch per component.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Input Access", "IA", "Input access levels (item/list/tree) — one branch per component.", GH_ParamAccess.tree);

            // Output param trees — one branch per component
            pManager.AddTextParameter("Output Names", "ON", "Output parameter names — one branch per component.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Output Nicknames", "ONK", "Output parameter nicknames — one branch per component.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Output Descriptions", "OD", "Output parameter descriptions — one branch per component.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Output Types", "OT", "Output parameter types — one branch per component.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Output Access", "OA", "Output access levels (item/list/tree) — one branch per component.", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var rawInputs = new List<IGH_Goo>();
            DA.GetDataList(0, rawInputs);

            // Resolve unique components from wired sources
            var comps = new List<IGH_Component>();
            foreach (var src in Params.Input[0].Sources)
            {
                var comp = src?.Attributes?.GetTopLevel?.DocObject as IGH_Component
                        ?? src?.Attributes?.DocObject as IGH_Component;
                if (comp != null && !comps.Contains(comp))
                    comps.Add(comp);
            }

            if (comps.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No components resolved.");
                return;
            }

            // Flat lists
            var names = new List<string>();
            var nicknames = new List<string>();
            var categories = new List<string>();
            var subCats = new List<string>();
            var descriptions = new List<string>();

            // Trees
            var inNames = new GH_Structure<GH_String>();
            var inNicks = new GH_Structure<GH_String>();
            var inDescs = new GH_Structure<GH_String>();
            var inTypes = new GH_Structure<GH_String>();
            var inAccess = new GH_Structure<GH_String>();

            var outNames = new GH_Structure<GH_String>();
            var outNicks = new GH_Structure<GH_String>();
            var outDescs = new GH_Structure<GH_String>();
            var outTypes = new GH_Structure<GH_String>();
            var outAccess = new GH_Structure<GH_String>();

            for (int i = 0; i < comps.Count; i++)
            {
                var comp = comps[i];
                var path = new GH_Path(i);

                // Get canonical nicknames from a fresh instance
                var nickMap = GetNickNames(comp);

                // ---- Flat ----
                names.Add(comp.Name);
                nicknames.Add(comp.NickName);
                categories.Add(comp.Category);
                subCats.Add(comp.SubCategory);
                descriptions.Add(comp.Description);

                // ---- Input params ----
                foreach (var p in comp.Params.Input)
                {
                    string nick = nickMap.TryGetValue(p.InstanceGuid, out var n) ? n : p.NickName;
                    inNames.Append(new GH_String(p.Name), path);
                    inNicks.Append(new GH_String(nick), path);
                    inDescs.Append(new GH_String(p.Description), path);
                    inTypes.Append(new GH_String(p.TypeName), path);
                    inAccess.Append(new GH_String(AccessLabel(p.Access)), path);
                }

                // ---- Output params ----
                foreach (var p in comp.Params.Output)
                {
                    string nick = nickMap.TryGetValue(p.InstanceGuid, out var n) ? n : p.NickName;
                    outNames.Append(new GH_String(p.Name), path);
                    outNicks.Append(new GH_String(nick), path);
                    outDescs.Append(new GH_String(p.Description), path);
                    outTypes.Append(new GH_String(p.TypeName), path);
                    outAccess.Append(new GH_String(AccessLabel(p.Access)), path);
                }
            }

            // Set flat lists
            DA.SetDataList(0, names);
            DA.SetDataList(1, nicknames);
            DA.SetDataList(2, categories);
            DA.SetDataList(3, subCats);
            DA.SetDataList(4, descriptions);

            // Set input trees
            DA.SetDataTree(5, inNames);
            DA.SetDataTree(6, inNicks);
            DA.SetDataTree(7, inDescs);
            DA.SetDataTree(8, inTypes);
            DA.SetDataTree(9, inAccess);

            // Set output trees
            DA.SetDataTree(10, outNames);
            DA.SetDataTree(11, outNicks);
            DA.SetDataTree(12, outDescs);
            DA.SetDataTree(13, outTypes);
            DA.SetDataTree(14, outAccess);
        }

        private static string AccessLabel(GH_ParamAccess access)
        {
            switch (access)
            {
                case GH_ParamAccess.item: return "item";
                case GH_ParamAccess.list: return "list";
                case GH_ParamAccess.tree: return "tree";
                default: return access.ToString().ToLower();
            }
        }

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

        protected override Bitmap Icon => Properties.Resources.NC_DocRawData;
        public override Guid ComponentGuid => new Guid("00C9FB11-1EEA-4EEC-BC00-F2630CE5078C");
    }
}