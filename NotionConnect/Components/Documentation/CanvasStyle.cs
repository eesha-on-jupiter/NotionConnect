using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Drawing;

namespace NotionConnect.Versioning
{
    /// <summary>
    /// Toggles the Grasshopper canvas into a clean presentation state —
    /// Notion off-white background, no wires, no grid — and back again.
    /// </summary>
    public class CanvasPresentComponent : ButtonComponent
    {
        public override string ButtonLabel => _presented ? "Restore" : "Style";

        private bool _presented = false;

        // Stored originals so can restore exactly
        private Color _origBack;
        private Color _origShade;
        private Color _origGrid;
        private Color _origWireDefault;
        private Color _origWireSelected;
        private Color _origWireEmpty;
        private Color _origWireError;
        private bool _origDrawGrid;

        private static readonly Color NotionOffWhite = Color.White;
        private static readonly Color Transparent = Color.FromArgb(0, 255, 255, 255);

        public CanvasPresentComponent()
          : base("Canvas Style", "CanvasStyle",
              "Toggles the canvas into a clean presentation state for screenshots. Press again to restore.",
              "NotionConnect", "Docs")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager) { }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Presenting", "P", "True while in presentation mode.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!IsTriggered)
            {
                DA.SetData(0, _presented);
                return;
            }

            if (!_presented)
                EnterPresentMode();
            else
                ExitPresentMode();

            DA.SetData(0, _presented);

            Grasshopper.Instances.ActiveCanvas?.Invalidate();
        }

        private void EnterPresentMode()
        {
            // Store originals
            _origBack = GH_Skin.canvas_back;
            _origShade = GH_Skin.canvas_shade;
            _origGrid = GH_Skin.canvas_grid;
            _origWireDefault = GH_Skin.wire_default;
            _origWireEmpty = GH_Skin.wire_empty;

            // Apply presentation skin
            GH_Skin.canvas_back = NotionOffWhite;
            GH_Skin.canvas_shade = NotionOffWhite;
            GH_Skin.canvas_grid = NotionOffWhite;  // grid invisible

            GH_Skin.wire_default = Transparent;
            GH_Skin.wire_empty = Transparent;

            _presented = true;
            RefreshCanvas();
        }

        private void ExitPresentMode()
        {
            GH_Skin.canvas_back = _origBack;
            GH_Skin.canvas_shade = _origShade;
            GH_Skin.canvas_grid = _origGrid;

            GH_Skin.wire_default = _origWireDefault;
            GH_Skin.wire_empty = _origWireEmpty;

            _presented = false;
            RefreshCanvas();
        }

        private static void RefreshCanvas()
        {
            Rhino.RhinoApp.InvokeOnUiThread((Action)(() =>
            {
                var canvas = Grasshopper.Instances.ActiveCanvas;
                if (canvas == null) return;
                canvas.Refresh();
            }));
        }

        protected override Bitmap Icon => Properties.Resources.NC_CanvasStyle;
        public override Guid ComponentGuid => new Guid("19448A31-4981-4827-A2AC-47B7A1926A97");
    }
}