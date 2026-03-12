using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NotionConnect
{
    public static class DatabasePropertyBuilders
    {
        private static string PropertyJson(string name, JObject definition)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = "Property";

            var obj = new JObject
            {
                ["name"] = name,
                ["definition"] = definition ?? new JObject()
            };

            return obj.ToString(Formatting.None);
        }

        public static JObject ParsePropertyJson(string propertyJson)
            => string.IsNullOrWhiteSpace(propertyJson) ? null : JObject.Parse(propertyJson);

        public static string Title(string name = "Name")
            => PropertyJson(name, new JObject { ["title"] = new JObject() });

        public static string RichText(string name)
            => PropertyJson(name, new JObject { ["rich_text"] = new JObject() });

        public static string Number(string name, string format = "number")
            => PropertyJson(name, new JObject
            {
                ["number"] = new JObject
                {
                    // If invalid, Notion may reject; keep default "number".
                    ["format"] = string.IsNullOrWhiteSpace(format) ? "number" : format
                }
            });

        public static string Checkbox(string name)
            => PropertyJson(name, new JObject { ["checkbox"] = new JObject() });

        public static string Date(string name)
            => PropertyJson(name, new JObject { ["date"] = new JObject() });

        public static string Url(string name)
            => PropertyJson(name, new JObject { ["url"] = new JObject() });

        public static string Email(string name)
            => PropertyJson(name, new JObject { ["email"] = new JObject() });

        public static string PhoneNumber(string name)
            => PropertyJson(name, new JObject { ["phone_number"] = new JObject() });

        public static string People(string name)
            => PropertyJson(name, new JObject { ["people"] = new JObject() });

        public static string Files(string name)
            => PropertyJson(name, new JObject { ["files"] = new JObject() });

        public static string Select(string name, IEnumerable<string> optionNames, IEnumerable<string> optionColors = null)
        {
            var options = BuildSelectOptions(optionNames, optionColors);
            return PropertyJson(name, new JObject
            {
                ["select"] = new JObject
                {
                    ["options"] = options
                }
            });
        }

        public static string MultiSelect(string name, IEnumerable<string> optionNames, IEnumerable<string> optionColors = null)
        {
            var options = BuildSelectOptions(optionNames, optionColors);
            return PropertyJson(name, new JObject
            {
                ["multi_select"] = new JObject
                {
                    ["options"] = options
                }
            });
        }

        private static JArray BuildSelectOptions(IEnumerable<string> optionNames, IEnumerable<string> optionColors)
        {
            var names = (optionNames ?? Enumerable.Empty<string>())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var colors = (optionColors ?? Enumerable.Empty<string>())
                .Select(c => string.IsNullOrWhiteSpace(c) ? "default" : c.Trim())
                .ToList();

            var arr = new JArray();

            for (int i = 0; i < names.Count; i++)
            {
                string color = (i < colors.Count) ? colors[i] : "default";

                arr.Add(new JObject
                {
                    ["name"] = names[i],
                    ["color"] = color
                });
            }

            return arr;
        }

        public static string Status(string name, IEnumerable<string> optionNames, IEnumerable<string> optionColors = null)
        {
            var options = BuildSelectOptions(optionNames, optionColors);

            return PropertyJson(name, new JObject
            {
                ["status"] = new JObject
                {
                    ["options"] = options
                }
            });
        }
    }
}