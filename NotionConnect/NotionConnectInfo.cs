using Grasshopper;
using Grasshopper.Kernel;
using Microsoft.VisualBasic;
using NotionConnect.Properties;
using System;
using System.Drawing;

namespace NotionConnect
{
    public class NotionConnectInfo : GH_AssemblyInfo
    {
        public override string Name => "NotionConnect";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;
        
        //Return a short string describing the purpose of this GHA library.
        public override string Description => "A Grasshopper plugin for reading and writing Notion pages and databases directly from parametric workflows. Also supports automated plugin documentation and GH file versioning.";

        public override Guid Id => new Guid("76ca3201-2551-476f-8e63-2767d67ddb24");

        //Return a string identifying you or your company.
        public override string AuthorName => "Eesha Jain";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "toeesha@gmail.com";

        //Return a string representing the version.  This returns the same version as the assembly.
        public override string AssemblyVersion => "1.0.0";
    }

    public class NotionConnectCategoryIcon : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Instances.ComponentServer.AddCategoryIcon("NotionConnect", Resources.NC_NotionConnectLogo);
            Instances.ComponentServer.AddCategorySymbolName("NotionConnect", 'N');
            return GH_LoadingInstruction.Proceed;
        }
    }
}