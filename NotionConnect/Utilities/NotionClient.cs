using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NotionConnect
{
    public enum NotionObjectKind { Database, Page }

    public sealed class NotionClient
    {
        private readonly HttpClient _http;
        private readonly string _token;
        private readonly string _notionVersion;

        public NotionClient(string token, string notionVersion = "2022-06-28")
        {
            _token = (token ?? "").Trim();
            _notionVersion = notionVersion;
            _http = new HttpClient { BaseAddress = new Uri("https://api.notion.com/v1/") };
        }

        private HttpRequestMessage NewRequest(HttpMethod method, string relativeUrl)
        {
            var req = new HttpRequestMessage(method, relativeUrl);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            req.Headers.Add("Notion-Version", _notionVersion);
            return req;
        }

        public async Task<Tuple<bool, string, string>> GetMeAsync()
        {
            try
            {
                var req = NewRequest(HttpMethod.Get, "users/me");
                var res = await _http.SendAsync(req).ConfigureAwait(false);
                string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!res.IsSuccessStatusCode)
                    return Fail("HTTP " + (int)res.StatusCode + " " + res.ReasonPhrase + "\n" + body);

                string name = ExtractJsonString(body, "\"name\"") ?? "(no name)";
                return Ok(name);
            }
            catch (Exception ex) { return Fail(ex.Message); }
        }

        public async Task<Tuple<bool, string[], string[], string>> SearchAsync(string query, NotionObjectKind kind)
        {
            try
            {
                var payload = new JObject();
                if (!string.IsNullOrWhiteSpace(query)) payload["query"] = query;
                payload["filter"] = new JObject
                {
                    ["value"] = kind == NotionObjectKind.Database ? "database" : "page",
                    ["property"] = "object"
                };

                var req = NewRequest(HttpMethod.Post, "search");
                req.Content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");

                var res = await _http.SendAsync(req).ConfigureAwait(false);
                string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!res.IsSuccessStatusCode)
                    return new Tuple<bool, string[], string[], string>(false, new string[0], new string[0],
                        "HTTP " + (int)res.StatusCode + " " + res.ReasonPhrase + "\n" + body);

                var root = JObject.Parse(body);
                var results = (JArray)root["results"];

                if (results == null || results.Count == 0)
                    return new Tuple<bool, string[], string[], string>(true, new string[0], new string[0], "No results.");

                var names = new List<string>();
                var ids = new List<string>();

                foreach (JObject obj in results)
                {
                    string id = obj.Value<string>("id");
                    string name = kind == NotionObjectKind.Database ? GetDatabaseTitle(obj) : GetPageTitle(obj);
                    ids.Add(id);
                    names.Add(string.IsNullOrWhiteSpace(name) ? "(untitled)" : name);
                }

                return new Tuple<bool, string[], string[], string>(true, names.ToArray(), ids.ToArray(), "");
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string[], string[], string>(false, new string[0], new string[0], ex.ToString());
            }
        }

        public async Task<Tuple<bool, string, string>> AppendBlocksAsync(string pageId, JArray children)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pageId)) return Fail("PageId is empty.");

                var payload = new JObject { ["children"] = children ?? new JArray() };
                var req = NewRequest(new HttpMethod("PATCH"), $"blocks/{pageId}/children");
                req.Content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");

                var res = await _http.SendAsync(req).ConfigureAwait(false);
                string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!res.IsSuccessStatusCode)
                    return Fail("HTTP " + (int)res.StatusCode + " " + res.ReasonPhrase + "\n" + body);

                return Ok(body);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        /// <summary>
        /// Uploads a file to Notion using the two-step direct upload flow:
        /// Step 1 — POST /v1/file_uploads (JSON) to create upload object, get id + upload_url
        /// Step 2 — POST to upload_url (/v1/file_uploads/{id}/send) with multipart file bytes
        /// Returns the file_id for use in Files property row values.
        /// </summary>
        public async Task<Tuple<bool, string, string>> UploadFileAsync(string fileName, byte[] fileBytes, string mimeType = "image/png")
        {
            try
            {
                if (fileBytes == null || fileBytes.Length == 0)
                    return Fail("File bytes are empty.");

                if (fileBytes.Length > 5 * 1024 * 1024)
                    return Fail("File exceeds Notion 5MB limit. Compress before uploading.");

                // STEP 1 — Create file upload object (JSON, not multipart)
                var createPayload = new JObject
                {
                    ["name"] = fileName ?? "upload",
                    ["mode"] = "single_part"
                };

                var createReq = NewRequest(HttpMethod.Post, "file_uploads");
                createReq.Content = new StringContent(
                    createPayload.ToString(Formatting.None),
                    Encoding.UTF8,
                    "application/json"
                );

                var createRes = await _http.SendAsync(createReq).ConfigureAwait(false);
                string createBody = await createRes.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!createRes.IsSuccessStatusCode)
                    return Fail("Create upload failed: HTTP " + (int)createRes.StatusCode + " " + createRes.ReasonPhrase + "" + createBody);

                var createJson = JObject.Parse(createBody);
                string fileId = createJson["id"]?.ToString();
                string uploadUrl = createJson["upload_url"]?.ToString();

                if (string.IsNullOrWhiteSpace(fileId))
                    return Fail("No file ID in create response: " + createBody);

                // Use upload_url if provided, otherwise construct from id
                if (string.IsNullOrWhiteSpace(uploadUrl))
                    uploadUrl = $"https://api.notion.com/v1/file_uploads/{fileId}/send";

                // STEP 2 — Send file bytes via multipart to /send endpoint
                using (var form = new MultipartFormDataContent())
                {
                    var fileContent = new ByteArrayContent(fileBytes);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
                    form.Add(fileContent, "file", fileName ?? "upload");

                    // Use absolute URL for the send endpoint
                    using (var sendReq = new HttpRequestMessage(HttpMethod.Post, uploadUrl))
                    {
                        sendReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                        sendReq.Headers.Add("Notion-Version", _notionVersion);
                        sendReq.Content = form;

                        var sendRes = await _http.SendAsync(sendReq).ConfigureAwait(false);
                        string sendBody = await sendRes.Content.ReadAsStringAsync().ConfigureAwait(false);

                        if (!sendRes.IsSuccessStatusCode)
                            return Fail("Send file failed: HTTP " + (int)sendRes.StatusCode + " " + sendRes.ReasonPhrase + "" + sendBody);
                    }
                }

                return Ok(fileId);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        /// <summary>
        /// Creates a new page as a child of an existing page.
        /// Returns the full response body (containing the new page ID) on success.
        /// </summary>
        public async Task<Tuple<bool, string, string>> CreatePageAsync(string parentPageId, string title)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(parentPageId)) return Fail("Parent page ID is empty.");

                var payload = new JObject
                {
                    ["parent"] = new JObject { ["type"] = "page_id", ["page_id"] = parentPageId.Trim() },
                    ["properties"] = new JObject
                    {
                        ["title"] = new JObject
                        {
                            ["title"] = new JArray
                            {
                                new JObject
                                {
                                    ["type"] = "text",
                                    ["text"] = new JObject { ["content"] = title ?? "" }
                                }
                            }
                        }
                    }
                };

                var req = NewRequest(HttpMethod.Post, "pages");
                req.Content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");

                var res = await _http.SendAsync(req).ConfigureAwait(false);
                string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!res.IsSuccessStatusCode)
                    return Fail("HTTP " + (int)res.StatusCode + " " + res.ReasonPhrase + "\n" + body);

                return Ok(body);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        public async Task<Tuple<bool, string, string>> CreateDatabaseAsync(string createDatabaseJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createDatabaseJson)) return Fail("CreateDatabaseJson is empty.");

                var req = NewRequest(HttpMethod.Post, "databases");
                req.Content = new StringContent(createDatabaseJson, Encoding.UTF8, "application/json");

                var res = await _http.SendAsync(req).ConfigureAwait(false);
                string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!res.IsSuccessStatusCode)
                    return Fail("HTTP " + (int)res.StatusCode + " " + res.ReasonPhrase + "\n" + body);

                return Ok(body);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        public async Task<Tuple<bool, string, string>> CreateRowAsync(string createRowJson)
        {
            try
            {
                var req = NewRequest(HttpMethod.Post, "pages");
                req.Content = new StringContent(createRowJson, Encoding.UTF8, "application/json");

                var res = await _http.SendAsync(req).ConfigureAwait(false);
                string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!res.IsSuccessStatusCode)
                    return Fail("HTTP " + (int)res.StatusCode + " " + res.ReasonPhrase + "\n" + body);

                return Ok(body);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        /// <summary>
        /// Queries a database and returns all page IDs of existing rows.
        /// Handles pagination automatically.
        /// </summary>
        public async Task<Tuple<bool, List<string>, string>> QueryDatabaseAsync(string databaseId)
        {
            try
            {
                databaseId = databaseId?.Trim();
                if (string.IsNullOrWhiteSpace(databaseId))
                    return new Tuple<bool, List<string>, string>(false, null, "DatabaseId is empty.");

                var allIds = new List<string>();
                string startCursor = null;
                bool hasMore = true;

                while (hasMore)
                {
                    var payload = new JObject();
                    if (startCursor != null)
                        payload["start_cursor"] = startCursor;

                    var req = NewRequest(HttpMethod.Post, $"databases/{databaseId}/query");
                    req.Content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");

                    var res = await _http.SendAsync(req).ConfigureAwait(false);
                    string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!res.IsSuccessStatusCode)
                        return new Tuple<bool, List<string>, string>(false, null,
                            "HTTP " + (int)res.StatusCode + " " + res.ReasonPhrase + "\n" + body);

                    var root = JObject.Parse(body);
                    var results = (JArray)root["results"];

                    if (results != null)
                        foreach (JObject page in results)
                        {
                            string id = page.Value<string>("id");
                            if (!string.IsNullOrWhiteSpace(id))
                                allIds.Add(id);
                        }

                    hasMore = root.Value<bool>("has_more");
                    startCursor = root.Value<string>("next_cursor");

                    // Respect rate limit between paginated calls
                    if (hasMore)
                        await Task.Delay(350).ConfigureAwait(false);
                }

                return new Tuple<bool, List<string>, string>(true, allIds, "");
            }
            catch (Exception ex)
            {
                return new Tuple<bool, List<string>, string>(false, null, ex.ToString());
            }
        }

        /// <summary>
        /// Archives a page (removes it from DB view). Notion has no true delete.
        /// </summary>
        public async Task<Tuple<bool, string, string>> ArchivePageAsync(string pageId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pageId)) return Fail("PageId is empty.");

                var payload = new JObject { ["archived"] = true };
                var req = NewRequest(new HttpMethod("PATCH"), $"pages/{pageId}");
                req.Content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");

                var res = await _http.SendAsync(req).ConfigureAwait(false);
                string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!res.IsSuccessStatusCode)
                    return Fail("HTTP " + (int)res.StatusCode + " " + res.ReasonPhrase + "\n" + body);

                return Ok(body);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        public async Task<Tuple<bool, string, string>> EraseContentOnPageAsync(string pageId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pageId))
                    return Fail("PageId is empty.");

                var payload = new JObject { ["erase_content"] = true };
                var req = NewRequest(new HttpMethod("PATCH"), $"pages/{pageId}");
                req.Content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");

                var res = await _http.SendAsync(req).ConfigureAwait(false);
                string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!res.IsSuccessStatusCode)
                    return Fail("HTTP " + (int)res.StatusCode + " " + res.ReasonPhrase + "\n" + body);

                return Ok(body);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        // ---- Helpers ----

        private static Tuple<bool, string, string> Ok(string body)
            => new Tuple<bool, string, string>(true, body, "");

        private static Tuple<bool, string, string> Fail(string error)
            => new Tuple<bool, string, string>(false, "", error);

        private static string ExtractJsonString(string json, string key)
        {
            int i = json.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (i < 0) return null;
            int colon = json.IndexOf(':', i);
            if (colon < 0) return null;
            int firstQuote = json.IndexOf('"', colon + 1);
            if (firstQuote < 0) return null;
            int secondQuote = json.IndexOf('"', firstQuote + 1);
            if (secondQuote < 0) return null;
            return json.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
        }

        private static string GetDatabaseTitle(JObject db)
        {
            var titleArr = db.SelectToken("title") as JArray;
            if (titleArr != null && titleArr.Count > 0)
            {
                var plain = titleArr[0]["plain_text"];
                if (plain != null) return plain.ToString();
            }
            return "(untitled)";
        }

        private static string GetPageTitle(JObject page)
        {
            var props = page["properties"] as JObject;
            if (props == null) return null;

            foreach (var p in props.Properties())
            {
                var propObj = p.Value as JObject;
                if (propObj == null) continue;
                if (propObj.Value<string>("type") != "title") continue;

                var titleArr = propObj["title"] as JArray;
                if (titleArr != null && titleArr.Count > 0)
                    return titleArr[0]["plain_text"]?.ToString() ?? "";
                return "";
            }
            return null;
        }
    }
}