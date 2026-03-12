using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace NotionConnect
{
    public static class DatabaseRowBuilders
    {
        private static JArray RichText(string content)
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

        public static JObject TitleValue(string text)
            => new JObject { ["title"] = RichText(text) };

        public static JObject RichTextValue(string text)
            => new JObject { ["rich_text"] = RichText(text) };

        public static JObject NumberValue(double value)
            => new JObject { ["number"] = value };

        public static JObject CheckboxValue(bool value)
            => new JObject { ["checkbox"] = value };

        public static JObject DateValue(string isoStart, string isoEnd = null)
        {
            var date = new JObject { ["start"] = isoStart };
            if (!string.IsNullOrWhiteSpace(isoEnd))
                date["end"] = isoEnd;

            return new JObject { ["date"] = date };
        }

        public static JObject SelectValue(string optionName)
            => new JObject { ["select"] = new JObject { ["name"] = optionName ?? "" } };

        public static JObject MultiSelectValue(IEnumerable<string> optionNames)
        {
            var arr = new JArray();
            if (optionNames != null)
            {
                foreach (var n in optionNames)
                {
                    if (string.IsNullOrWhiteSpace(n)) continue;
                    arr.Add(new JObject { ["name"] = n.Trim() });
                }
            }
            return new JObject { ["multi_select"] = arr };
        }

        public static JObject UrlValue(string url)
            => new JObject { ["url"] = url ?? "" };

        public static JObject EmailValue(string email)
            => new JObject { ["email"] = email ?? "" };

        public static JObject PhoneValue(string phone)
            => new JObject { ["phone_number"] = phone ?? "" };

        public static string CreateRowJson(string databaseId, IDictionary<string, JObject> propertyValues)
        {
            if (string.IsNullOrWhiteSpace(databaseId))
                throw new ArgumentException("databaseId is required.");

            var props = new JObject();
            if (propertyValues != null)
            {
                foreach (var kv in propertyValues)
                {
                    if (string.IsNullOrWhiteSpace(kv.Key) || kv.Value == null) continue;
                    props[kv.Key] = kv.Value;
                }
            }

            var body = new JObject
            {
                ["parent"] = new JObject { ["database_id"] = databaseId },
                ["properties"] = props
            };

            return body.ToString(Formatting.None);
        }

        public static string ParseRowPageId(string responseJson)
        {
            if (string.IsNullOrWhiteSpace(responseJson)) return null;
            return JObject.Parse(responseJson)["id"]?.ToString();
        }
    }
}