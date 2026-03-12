using Grasshopper.Kernel;
using System;

namespace NotionConnect
{
    public class SearchDatabaseComponent : GH_Component
    {
        public SearchDatabaseComponent()
          : base("Search Database", "SearchDatabase",
              "Search for Notion databases by name.",
              "NotionConnect", "Auth")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Notion internal integration token.", GH_ParamAccess.item);
            pManager.AddTextParameter("Query", "Q", "Optional search query.", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Names", "N", "Database names.", GH_ParamAccess.list);
            pManager.AddTextParameter("Database ID", "DID", "Database IDs.", GH_ParamAccess.list);
            pManager.AddTextParameter("Error", "E", "Error message, if any.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string token = "";
            string query = "";

            if (!DA.GetData(0, ref token) || string.IsNullOrWhiteSpace(token))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Token is empty.");
                DA.SetDataList(0, new string[0]);
                DA.SetDataList(1, new string[0]);
                DA.SetData(2, "Token is empty.");
                return;
            }

            DA.GetData(1, ref query);

            try
            {
                var client = new NotionClient(token);
                var result = client.SearchAsync(query, NotionObjectKind.Database)
                    .ConfigureAwait(false).GetAwaiter().GetResult();

                if (!result.Item1)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, result.Item4);
                    DA.SetDataList(0, new string[0]);
                    DA.SetDataList(1, new string[0]);
                    DA.SetData(2, result.Item4);
                    return;
                }

                DA.SetDataList(0, result.Item2);
                DA.SetDataList(1, result.Item3);
                DA.SetData(2, result.Item4);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                DA.SetDataList(0, new string[0]);
                DA.SetDataList(1, new string[0]);
                DA.SetData(2, ex.Message);
            }
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.NC_SearchDatabase;
        public override Guid ComponentGuid => new Guid("A5AA5C95-23A6-48D8-A873-FCF728CFE977");
    }
}