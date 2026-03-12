using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace NotionConnect
{
    public static class BlockBuilders
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

        // Generic: Repatable code to build blocks
        private static string CustomBlockJson(string type, JObject innerContent)
        {
            JObject block = new JObject
            {
                ["object"] = "block",
                ["type"] = type,
                [type] = innerContent ?? new JObject()
            };

            return block.ToString(Formatting.None);
        }

        private static string SimpleBlockJson(string type)
            => CustomBlockJson(type, new JObject());

        private static string RichTextBlockJson(string type, string text)
            => CustomBlockJson(type, new JObject { ["rich_text"] = RichText(text) });

        //Wrappers call certain blocks
        public static string ParagraphJson(string text)
            => RichTextBlockJson("paragraph", text);

        public static string HeadingJson(string text, int level)
        {
            string type =
                (level <= 1) ? "heading_1" :
                (level == 2) ? "heading_2" :
                               "heading_3";

            return RichTextBlockJson(type, text);
        }

        public static string BulletedItemJson(string text)
            => RichTextBlockJson("bulleted_list_item", text);

        public static string NumberedItemJson(string text)
            => RichTextBlockJson("numbered_list_item", text);

        public static string QuoteJson(string text)
            => RichTextBlockJson("quote", text);

        public static string CalloutJson(string text, string emoji = "💡")
            => CustomBlockJson("callout", new JObject
            {
                ["rich_text"] = RichText(text),
                ["icon"] = new JObject { ["type"] = "emoji", ["emoji"] = emoji }
            });

        public static string CodeJson(string code, string language = "plain text")
            => CustomBlockJson("code", new JObject
            {
                ["rich_text"] = RichText(code),
                ["language"] = language
            });

        public static string ToggleJson(string text, string color = "default", JArray children = null)
        {
            var inner = new JObject
            {
                ["rich_text"] = RichText(text),
                ["color"] = string.IsNullOrWhiteSpace(color) ? "default" : color
            };
            if (children != null && children.Count > 0)
                inner["children"] = children;

            return CustomBlockJson("toggle", inner);
        }

        public static string ToDoJson(string text, bool isChecked = false, string color = "default")
        {
            var inner = new JObject
            {
                ["rich_text"] = RichText(text),
                ["checked"] = isChecked,
                ["color"] = string.IsNullOrWhiteSpace(color) ? "default" : color
            };

            return CustomBlockJson("to_do", inner);
        }

        public static string TableJson(
            System.Collections.Generic.List<System.Collections.Generic.List<string>> rows,
            int tableWidth,
            bool hasColumnHeader = false,
            bool hasRowHeader = false)
        {
            if (tableWidth < 1) tableWidth = 1;

            JArray children = new JArray();

            // Notion needs table_row children; ensure at least one row
            if (rows == null || rows.Count == 0)
                rows = new System.Collections.Generic.List<System.Collections.Generic.List<string>> { new System.Collections.Generic.List<string>() };

            foreach (var r in rows)
            {
                JArray cells = new JArray();

                for (int c = 0; c < tableWidth; c++)
                {
                    string txt = (r != null && c < r.Count) ? r[c] : "";
                    // each cell is an ARRAY of rich_text objects
                    cells.Add(RichText(txt));
                }

                children.Add(new JObject
                {
                    ["object"] = "block",
                    ["type"] = "table_row",
                    ["table_row"] = new JObject
                    {
                        ["cells"] = cells
                    }
                });
            }

            var inner = new JObject
            {
                ["table_width"] = tableWidth,
                ["has_column_header"] = hasColumnHeader,
                ["has_row_header"] = hasRowHeader,
                ["children"] = children
            };

            return CustomBlockJson("table", inner);
        }

        public static string DividerJson()
            => SimpleBlockJson("divider");

        public static string EmbedJson(string url, string caption = "")
            => CustomBlockJson("embed", new JObject
            {
                ["url"] = url ?? "",
                ["caption"] = RichText(caption ?? "")
            });

        public static string BookmarkJson(string url, string caption = "")
            => CustomBlockJson("bookmark", new JObject
            {
                ["url"] = url ?? "",
                ["caption"] = RichText(caption ?? "")
            });

        public static string VideoBlockJson(string url, string caption = "")
            => CustomBlockJson("video", new JObject
            {
                ["type"] = "external",
                ["external"] = new JObject { ["url"] = url ?? "" },
                ["caption"] = RichText(caption ?? "")
            });

        public static string TableOfContentsJson(string color = "default")
            => CustomBlockJson("table_of_contents", new JObject
            {
                ["color"] = string.IsNullOrWhiteSpace(color) ? "default" : color
            });

        public static string ColumnListJson(JArray columns)
            => CustomBlockJson("column_list", new JObject
            {
                ["children"] = columns ?? new JArray()
            });

        public static string ColumnJson(JArray children)
            => CustomBlockJson("column", new JObject
            {
                ["children"] = children ?? new JArray()
            });

        public static string ImageBlockJson(string url, string fileId = null, string caption = "")
        {
            JObject imageInner;

            if (!string.IsNullOrWhiteSpace(fileId))
            {
                // Notion-hosted uploaded file
                imageInner = new JObject
                {
                    ["type"] = "file_upload",
                    ["file_upload"] = new JObject { ["id"] = fileId },
                    ["caption"] = RichText(caption ?? "")
                };
            }
            else
            {
                // External URL
                imageInner = new JObject
                {
                    ["type"] = "external",
                    ["external"] = new JObject { ["url"] = url ?? "" },
                    ["caption"] = RichText(caption ?? "")
                };
            }

            return CustomBlockJson("image", imageInner);
        }

        public static string FileBlockJson(string url, string fileId = null, string caption = "")
        {
            JObject fileInner;

            if (!string.IsNullOrWhiteSpace(fileId))
            {
                fileInner = new JObject
                {
                    ["type"] = "file_upload",
                    ["file_upload"] = new JObject { ["id"] = fileId },
                    ["caption"] = RichText(caption ?? "")
                };
            }
            else
            {
                fileInner = new JObject
                {
                    ["type"] = "external",
                    ["external"] = new JObject { ["url"] = url ?? "" },
                    ["caption"] = RichText(caption ?? "")
                };
            }

            return CustomBlockJson("file", fileInner);
        }

        public static string Heading3Json(string text, string color = "default")
            => CustomBlockJson("heading_3", new JObject
            {
                ["rich_text"] = RichText(text),
                ["color"] = string.IsNullOrWhiteSpace(color) ? "default" : color
            });

        public static JObject ParseBlockJson(string blockJson)
            => string.IsNullOrWhiteSpace(blockJson) ? null : JObject.Parse(blockJson);

    }
}