using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;

namespace NotionConnect.Versioning
{
    public class VersionSaveComponent : ButtonComponent
    {
        public override string ButtonLabel => "Push";

        // ── Cached row values — survive re-solve after IncrementAndRefresh ──
        private string _cachedVersion = null;
        private string _cachedDate = null;
        private string _cachedPerson = null;
        private string _cachedFile = null;
        private bool _cachedOk = false;
        private string _cachedError = "";

        // ── Static schema definitions (HJ) — always the same ──
        private static readonly List<string> Headings = new List<string>
        {
            "{\"name\":\"Version\",\"type\":\"title\",\"title\":{}}",
            "{\"name\":\"Date\",\"type\":\"date\",\"date\":{}}",
            "{\"name\":\"Person\",\"type\":\"people\",\"people\":{}}",
            "{\"name\":\"File\",\"type\":\"files\",\"files\":{}}"
        };

        public VersionSaveComponent()
          : base("Version Save", "Version Save",
              "Saves and zips the .gh file, outputs HJ + RJ directly into Database Assembler.",
              "NotionConnect", "Versioning")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Notion internal integration token.", GH_ParamAccess.item);
            pManager.AddTextParameter("Version Name", "VN", "Version name — wire from Version Name component.", GH_ParamAccess.item);
            pManager.AddTextParameter("Person ID", "PID", "Notion user ID — wire from Notion Users.", GH_ParamAccess.item, "");
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("HeadingJson", "HJ", "Schema definitions — wire into Database Assembler HJ.", GH_ParamAccess.list);
            pManager.AddTextParameter("RowJson", "RJ", "Row values tree — wire into Database Assembler RJ.", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("OK", "OK", "True if push succeeded.", GH_ParamAccess.item);
            pManager.AddTextParameter("Error", "E", "Error message, if any.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string token = null;
            string versionName = null;
            string personId = "";

            if (!DA.GetData(0, ref token)) return;
            if (!DA.GetData(1, ref versionName)) return;
            DA.GetData(2, ref personId);

            token = token?.Trim();
            personId = personId?.Trim() ?? "";

            // ── Run push only when button is pressed ──
            if (IsTriggered)
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    SetCache(null, null, null, null, false, "Token required.");
                }
                else
                {
                    string ghFilePath = OnPingDocument()?.FilePath;
                    if (string.IsNullOrWhiteSpace(ghFilePath) || !File.Exists(ghFilePath))
                    {
                        SetCache(null, null, null, null, false, "Save the .gh file to disk first.");
                    }
                    else
                    {
                        try
                        {
                            var ghDoc = OnPingDocument();
                            if (ghDoc != null)
                                new GH_DocumentIO(ghDoc).SaveQuiet(ghDoc.FilePath);

                            var client = new NotionClient(token);
                            byte[] zip = ZipGhFile(ghFilePath);
                            string date = DateTime.UtcNow.ToString("yyyy-MM-dd");
                            string zipName = $"{Path.GetFileNameWithoutExtension(ghFilePath)}_{versionName}.zip";

                            var (success, fileId, errMsg) = client.UploadFileAsync(zipName, zip, "application/zip")
                                                                   .GetAwaiter().GetResult();

                            if (!success)
                            {
                                SetCache(null, null, null, null, false, $"Upload failed: {errMsg}");
                            }
                            else
                            {
                                SetCache(
                                    rv: new JObject
                                    {
                                        ["title"] = new JArray
                                        {
                                            new JObject { ["type"] = "text", ["text"] = new JObject { ["content"] = versionName } }
                                        }
                                    }.ToString(Newtonsoft.Json.Formatting.None),

                                    rd: new JObject
                                    {
                                        ["date"] = new JObject { ["start"] = date }
                                    }.ToString(Newtonsoft.Json.Formatting.None),

                                    rp: new JObject
                                    {
                                        ["people"] = string.IsNullOrWhiteSpace(personId)
                                            ? new JArray()
                                            : new JArray { new JObject { ["object"] = "user", ["id"] = personId } }
                                    }.ToString(Newtonsoft.Json.Formatting.None),

                                    rf: new JObject
                                    {
                                        ["files"] = new JArray
                                        {
                                            new JObject
                                            {
                                                ["type"]        = "file_upload",
                                                ["name"]        = zipName,
                                                ["file_upload"] = new JObject { ["id"] = fileId }
                                            }
                                        }
                                    }.ToString(Newtonsoft.Json.Formatting.None),

                                    ok: true,
                                    error: ""
                                );

                                // Increment version — read prefix directly from the component
                                if (ghDoc != null)
                                    foreach (var obj in ghDoc.Objects)
                                        if (obj is VersionNameComponent vn)
                                        {
                                            string vnPrefix = "v";
                                            vn.Params.Input[0].CollectData();
                                            var prefixData = vn.Params.Input[0].VolatileData;
                                            if (prefixData.DataCount > 0)
                                                vnPrefix = (prefixData.get_Branch(prefixData.Paths[0])[0] as Grasshopper.Kernel.Types.GH_String)?.Value ?? "v";
                                            vn.IncrementAndRefresh(vnPrefix);
                                            break;
                                        }
                            }
                        }
                        catch (Exception ex)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                            SetCache(null, null, null, null, false, ex.Message);
                        }
                    }
                }
            }

            // ── HJ — always output schema headings ──
            DA.SetDataList(0, Headings);

            // ── RJ — one branch (path {0}), four values in column order ──
            var rjTree = new GH_Structure<GH_String>();
            if (_cachedVersion != null)
            {
                var path = new GH_Path(0);
                rjTree.Append(new GH_String(_cachedVersion), path);
                rjTree.Append(new GH_String(_cachedDate), path);
                rjTree.Append(new GH_String(_cachedPerson), path);
                rjTree.Append(new GH_String(_cachedFile), path);
            }
            DA.SetDataTree(1, rjTree);

            DA.SetData(2, _cachedOk);
            DA.SetData(3, _cachedError);
        }

        private void SetCache(string rv, string rd, string rp, string rf, bool ok, string error)
        {
            _cachedVersion = rv;
            _cachedDate = rd;
            _cachedPerson = rp;
            _cachedFile = rf;
            _cachedOk = ok;
            _cachedError = error;
        }

        private static byte[] ZipGhFile(string path)
        {
            using (var ms = new MemoryStream())
            {
                using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
                {
                    var entry = zip.CreateEntry(Path.GetFileName(path), CompressionLevel.Fastest);
                    using (var s = entry.Open())
                    {
                        byte[] b = File.ReadAllBytes(path);
                        s.Write(b, 0, b.Length);
                    }
                }
                return ms.ToArray();
            }
        }

        protected override Bitmap Icon => Properties.Resources.NC_VersionSave;
        public override Guid ComponentGuid => new Guid("DD3A99A1-6D08-4A91-89BE-DFCB4C776DFE");
    }
}