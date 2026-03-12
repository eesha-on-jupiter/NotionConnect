using Grasshopper.Kernel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;

namespace NotionConnect
{
    public class NotionUsersComponent : GH_Component
    {
        public NotionUsersComponent()
          : base("Notion Users", "Notion Users",
              "Fetches all users in your Notion workspace. Wire a user ID into Version Save Person input.",
              "NotionConnect", "Auth")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Notion internal integration token.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Names", "N", "Workspace member display names.", GH_ParamAccess.list);
            pManager.AddTextParameter("IDs", "ID", "Notion user IDs — parallel to Names.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string token = null;
            if (!DA.GetData(0, ref token)) return;
            token = token?.Trim();

            if (string.IsNullOrWhiteSpace(token))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Token required.");
                return;
            }

            try
            {
                var (names, ids) = FetchUsers(token);
                DA.SetDataList(0, names);
                DA.SetDataList(1, ids);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
            }
        }

        private static (List<string> names, List<string> ids) FetchUsers(string token)
        {
            var names = new List<string>();
            var ids = new List<string>();

            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                http.DefaultRequestHeaders.Add("Notion-Version", "2022-06-28");

                string cursor = null;

                while (true)
                {
                    string url = "https://api.notion.com/v1/users";
                    if (cursor != null)
                        url += $"?start_cursor={cursor}";

                    var response = http.GetAsync(url).GetAwaiter().GetResult();
                    var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    if (!response.IsSuccessStatusCode)
                        throw new Exception($"Notion API error {(int)response.StatusCode}: {body}");

                    var json = JObject.Parse(body);
                    var results = json["results"] as JArray;

                    if (results != null)
                    {
                        foreach (var user in results)
                        {
                            if (user["type"]?.ToString() != "person") continue;

                            string name = user["name"]?.ToString() ?? "Unknown";
                            string id = user["id"]?.ToString() ?? "";

                            if (!string.IsNullOrWhiteSpace(id))
                            {
                                names.Add(name);
                                ids.Add(id);
                            }
                        }
                    }

                    bool hasMore = json["has_more"]?.Value<bool>() ?? false;
                    if (!hasMore) break;

                    cursor = json["next_cursor"]?.ToString();
                    if (string.IsNullOrWhiteSpace(cursor)) break;
                }
            }

            return (names, ids);
        }

        protected override Bitmap Icon => Properties.Resources.NC_NotionUser;
        public override Guid ComponentGuid => new Guid("9330DE17-4F75-4C2F-961B-3EC8DAD06F1F");
    }
}