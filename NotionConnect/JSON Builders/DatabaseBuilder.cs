using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace NotionConnect
{
    public static class DatabaseBuilders
    {
        private static JArray TitleRichText(string content)
        {
            return new JArray
            {
                new JObject
                {
                    ["type"] = "text",
                    ["text"] = new JObject { ["content"] = content ?? "" }
                }
            };
        }

        /// <summary>
        /// Accepts property JSON in either format:
        ///
        ///   Flat (from VersionSaveComponent / heading outputs):
        ///     { "name": "Version", "type": "title", "title": {} }
        ///
        ///   Wrapped (from DB.Prop components):
        ///     { "name": "Status", "definition": { "select": {} } }
        ///
        /// Builds a Notion-compatible properties object:
        ///     { "Version": { "title": {} }, "Status": { "select": {} } }
        /// </summary>
        public static JObject BuildPropertiesObject(IEnumerable<string> propertyJsonList, out bool hasTitle)
        {
            hasTitle = false;
            var props = new JObject();

            if (propertyJsonList == null) return props;

            foreach (var propJson in propertyJsonList)
            {
                if (string.IsNullOrWhiteSpace(propJson)) continue;

                JObject prop;
                try { prop = JObject.Parse(propJson); }
                catch { continue; }

                string name = prop["name"]?.ToString();
                if (string.IsNullOrWhiteSpace(name)) continue;

                JObject definition = null;

                if (prop["definition"] is JObject wrapped)
                {
                    // Wrapped format: { "name": "...", "definition": { "select": {} } }
                    definition = wrapped;
                }
                else if (prop["type"] is JToken typeToken)
                {
                    // Flat format: { "name": "...", "type": "title", "title": {} }
                    // Extract just the type key and its value as the definition
                    string typeName = typeToken.ToString();
                    if (!string.IsNullOrWhiteSpace(typeName) && prop[typeName] != null)
                    {
                        definition = new JObject
                        {
                            [typeName] = prop[typeName]
                        };
                    }
                }

                if (definition == null) continue;

                props[name] = definition;

                if (definition["title"] != null)
                    hasTitle = true;
            }

            return props;
        }

        /// <summary>
        /// Builds the POST /v1/databases request JSON body as a string.
        /// </summary>
        public static string CreateDatabaseJson(string parentPageId, string databaseName, IEnumerable<string> propertyJsonList)
        {
            if (string.IsNullOrWhiteSpace(parentPageId))
                throw new ArgumentException("parentPageId is required.");

            if (string.IsNullOrWhiteSpace(databaseName))
                databaseName = "New Database";

            JObject properties = BuildPropertiesObject(propertyJsonList, out bool hasTitle);

            if (!hasTitle)
                throw new ArgumentException("Database must include a Title property (e.g. DB.Prop.Title(\"Name\")).");

            var body = new JObject
            {
                ["parent"] = new JObject { ["page_id"] = parentPageId },
                ["title"] = TitleRichText(databaseName),
                ["properties"] = properties
            };

            return body.ToString(Formatting.None);
        }

        /// <summary>
        /// Parses a Notion database creation response and returns the database ID.
        /// </summary>
        public static string ParseDatabaseId(string responseJson)
        {
            if (string.IsNullOrWhiteSpace(responseJson)) return null;
            return JObject.Parse(responseJson)["id"]?.ToString();
        }
    }
}