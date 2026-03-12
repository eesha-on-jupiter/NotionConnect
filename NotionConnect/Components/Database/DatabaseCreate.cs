using Grasshopper.Kernel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace NotionConnect.Components.Database
{
    public class DatabaseCreateComponent : ButtonComponent
    {
        private static readonly string CacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NotionConnect");
        private static readonly string CachePath = Path.Combine(CacheDir, "db_cache.json");

        public override string ButtonLabel => "Create";

        public DatabaseCreateComponent()
          : base("Database Create", "DB Create",
              "Creates a Notion database. Press the button to create or recreate.",
              "NotionConnect", "Database")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Notion internal integration token.", GH_ParamAccess.item);
            pManager.AddTextParameter("Page ID", "PID", "Parent page ID.", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "Database name.", GH_ParamAccess.item, "New Database");
            pManager.AddTextParameter("HeadingJson", "HJ", "Property header definition JSONs — accepts list or tree, flattened automatically.", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Database ID", "DID", "Database ID — verified live against Notion.", GH_ParamAccess.item);
            pManager.AddTextParameter("Error", "E", "Status or error message.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string token = null;
            string parentId = null;
            string dbName = "New Database";
            var propJsons = new List<string>();
            Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.GH_String> hjTree;

            if (!DA.GetData(0, ref token)) return;
            if (!DA.GetData(1, ref parentId)) return;
            DA.GetData(2, ref dbName);
            DA.GetDataTree(3, out hjTree);

            // Flatten all branches into a single list
            foreach (var branch in hjTree.Branches)
                foreach (var item in branch)
                    if (item != null && !string.IsNullOrWhiteSpace(item.Value))
                        propJsons.Add(item.Value);

            token = token?.Trim();
            parentId = parentId?.Trim();

            string cacheKey = $"{parentId}::{dbName?.Trim()}";

            if (!IsTriggered)
            {
                string cachedId = ReadCache(cacheKey);

                if (!string.IsNullOrWhiteSpace(cachedId))
                {
                    // Verify the cached ID still exists on Notion
                    bool exists = DatabaseExists(token, cachedId);

                    if (exists)
                    {
                        DA.SetData(0, cachedId);
                        DA.SetData(1, "Using verified cached DB ID.");
                    }
                    else
                    {
                        // Stale — clear it so user knows to recreate
                        ClearCache(cacheKey);
                        DA.SetData(1, "Cached DB no longer exists on Notion. Press button to recreate.");
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Cached database ID is stale — press Create to recreate.");
                    }
                }
                else
                {
                    DA.SetData(1, "No cached DB found. Press button to create.");
                }
                return;
            }

            if (string.IsNullOrWhiteSpace(token)) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Token is required."); return; }
            if (string.IsNullOrWhiteSpace(parentId)) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Page ID is required."); return; }

            try
            {
                string bodyJson = DatabaseBuilders.CreateDatabaseJson(parentId, dbName, propJsons);
                var client = new NotionClient(token);
                var res = client.CreateDatabaseAsync(bodyJson).GetAwaiter().GetResult();

                if (!res.Item1)
                {
                    DA.SetData(1, res.Item3);
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, res.Item3);
                    return;
                }

                string dbId = DatabaseBuilders.ParseDatabaseId(res.Item2);

                if (string.IsNullOrWhiteSpace(dbId))
                {
                    DA.SetData(1, "Created but could not parse DB ID.");
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not parse DB ID.");
                    return;
                }

                // Always overwrite cache with the freshly created ID
                WriteCache(cacheKey, dbId);
                DA.SetData(0, dbId);
                DA.SetData(1, "Database created and cached.");
            }
            catch (Exception ex)
            {
                DA.SetData(1, ex.Message);
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.ToString());
            }
        }

        /// Pings GET /v1/databases/{id} — returns true if Notion responds 200.
        /// Returns false on 404, archived, or any error.
        private static bool DatabaseExists(string token, string dbId)
        {
            try
            {
                using (var http = new HttpClient())
                {
                    http.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                    http.DefaultRequestHeaders.Add("Notion-Version", "2022-06-28");

                    var res = http.GetAsync($"https://api.notion.com/v1/databases/{dbId}")
                                   .GetAwaiter().GetResult();
                    if (!res.IsSuccessStatusCode) return false;

                    // Also check if the DB has been archived
                    string body = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var json = JObject.Parse(body);
                    bool archived = json["archived"]?.Value<bool>() ?? false;
                    bool inTrash = json["in_trash"]?.Value<bool>() ?? false;

                    return !archived && !inTrash;
                }
            }
            catch { return false; }
        }

        private static string ReadCache(string key)
        {
            try
            {
                if (!File.Exists(CachePath)) return null;
                return JObject.Parse(File.ReadAllText(CachePath))[key]?.ToString();
            }
            catch { return null; }
        }

        private static void WriteCache(string key, string dbId)
        {
            try
            {
                Directory.CreateDirectory(CacheDir);
                JObject cache = new JObject();
                if (File.Exists(CachePath))
                    try { cache = JObject.Parse(File.ReadAllText(CachePath)); } catch { }
                cache[key] = dbId;
                File.WriteAllText(CachePath, cache.ToString(Formatting.Indented));
            }
            catch { }
        }

        private static void ClearCache(string key)
        {
            try
            {
                if (!File.Exists(CachePath)) return;
                JObject cache = new JObject();
                try { cache = JObject.Parse(File.ReadAllText(CachePath)); } catch { return; }
                cache.Remove(key);
                File.WriteAllText(CachePath, cache.ToString(Formatting.Indented));
            }
            catch { }
        }

        protected override Bitmap Icon => Properties.Resources.NC_DBCreate;
        public override Guid ComponentGuid => new Guid("94747C3C-3037-405E-99BA-72270D9C3637");
    }
}