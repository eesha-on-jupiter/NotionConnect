using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using Grasshopper.Kernel;

namespace NotionConnect.Docs
{
    public static class DocBlockBuilders
    {
        public static JArray RichTextArray(string text) => new JArray
        {
            new JObject
            {
                ["type"] = "text",
                ["text"] = new JObject { ["content"] = text ?? "" }
            }
        };

        public static JObject MakeRow(JArray cells) => new JObject
        {
            ["object"] = "block",
            ["type"] = "table_row",
            ["table_row"] = new JObject { ["cells"] = cells }
        };

        public static JArray MakeCell(string text, bool bold = false) => new JArray
        {
            new JObject
            {
                ["type"] = "text",
                ["text"] = new JObject { ["content"] = text ?? "" },
                ["annotations"] = new JObject
                {
                    ["bold"] = bold, ["italic"] = false, ["color"] = "default"
                }
            }
        };

        /// Plain paragraph block with optional italic and colour annotations.
        public static string ParagraphJson(string text, bool italic = false, string color = "default")
            => new JObject
            {
                ["object"] = "block",
                ["type"] = "paragraph",
                ["paragraph"] = new JObject
                {
                    ["rich_text"] = new JArray
                    {
                        new JObject
                        {
                            ["type"] = "text",
                            ["text"] = new JObject { ["content"] = text ?? "" },
                            ["annotations"] = new JObject
                            {
                                ["italic"] = italic,
                                ["color"]  = color
                            }
                        }
                    }
                }
            }.ToString(Newtonsoft.Json.Formatting.None);

        /// Two-line meta block: description on line 1, "Category > SubCategory" on line 2.
        /// Rendered as a single paragraph with a line break between them.
        public static string MetaLineJson(string description, string category, string subCategory)
            => new JObject
            {
                ["object"] = "block",
                ["type"] = "paragraph",
                ["paragraph"] = new JObject
                {
                    ["rich_text"] = new JArray
                    {
                        new JObject
                        {
                            ["type"] = "text",
                            ["text"] = new JObject { ["content"] = (description ?? "") + "\n" },
                            ["annotations"] = new JObject
                            {
                                ["italic"] = true,
                                ["color"]  = "gray"
                            }
                        },
                        new JObject
                        {
                            ["type"] = "text",
                            ["text"] = new JObject { ["content"] = $"{category} › {subCategory}" },
                            ["annotations"] = new JObject
                            {
                                ["italic"] = false,
                                ["bold"]   = false,
                                ["color"]  = "gray"
                            }
                        }
                    }
                }
            }.ToString(Newtonsoft.Json.Formatting.None);

        public static string BuildParamTable(IList<IGH_Param> parms, Dictionary<Guid, string> nickNames)
        {
            var rows = new JArray
            {
                MakeRow(new JArray
                {
                    MakeCell("Name",        bold: true),
                    MakeCell("Nickname",    bold: true),
                    MakeCell("Type",        bold: true),
                    MakeCell("Description", bold: true)
                })
            };

            foreach (var p in parms)
            {
                string nick = nickNames.TryGetValue(p.InstanceGuid, out var n) ? n : (p.NickName ?? "");
                rows.Add(MakeRow(new JArray
                {
                    MakeCell(p.Name        ?? ""),
                    MakeCell(nick),
                    MakeCell(p.TypeName    ?? ""),
                    MakeCell(p.Description ?? "")
                }));
            }

            return new JObject
            {
                ["object"] = "block",
                ["type"] = "table",
                ["table"] = new JObject
                {
                    ["table_width"] = 4,
                    ["has_column_header"] = true,
                    ["has_row_header"] = false,
                    ["children"] = rows
                }
            }.ToString(Newtonsoft.Json.Formatting.None);
        }
    }
}