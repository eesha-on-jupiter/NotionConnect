using Grasshopper.Kernel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace NotionConnect
{
    public static class docCapture
    {
        /// <summary>
        /// Captures a screenshot of a single GH component on the canvas,
        /// with an optional padding (in canvas units) around its bounds.
        /// Must be called on the UI thread.
        /// </summary>
        public static Bitmap CaptureComponent(
            Grasshopper.GUI.Canvas.GH_Canvas canvas,
            IGH_Component comp,
            int padding = 8)
        {
            try
            {
                var bounds = comp.Attributes?.Bounds ?? System.Drawing.RectangleF.Empty;
                if (bounds.IsEmpty) return null;

                var padded = new System.Drawing.RectangleF(
                    bounds.X - padding,
                    bounds.Y - padding,
                    bounds.Width + padding * 2,
                    bounds.Height + padding * 2);

                var xform = canvas.Viewport.XFormMatrix(
                    Grasshopper.GUI.Canvas.GH_Viewport.GH_DisplayMatrix.CanvasToControl);

                var pts = new System.Drawing.PointF[]
                {
                    padded.Location,
                    new System.Drawing.PointF(padded.Right, padded.Bottom)
                };
                xform.TransformPoints(pts);

                var cr = System.Drawing.Rectangle.Round(
                    System.Drawing.RectangleF.FromLTRB(pts[0].X, pts[0].Y, pts[1].X, pts[1].Y));
                cr = System.Drawing.Rectangle.Intersect(cr, canvas.ClientRectangle);
                if (cr.Width <= 0 || cr.Height <= 0) return null;

                var full = new Bitmap(
                    canvas.ClientRectangle.Width,
                    canvas.ClientRectangle.Height,
                    PixelFormat.Format32bppArgb);

                canvas.DrawToBitmap(full, canvas.ClientRectangle);

                var cropped = full.Clone(cr, PixelFormat.Format32bppArgb);
                full.Dispose();
                return cropped;
            }
            catch { return null; }
        }

        /// <summary>
        /// Captures the full visible canvas area.
        /// Must be called on the UI thread.
        /// </summary>
        public static Bitmap CaptureCanvas(Grasshopper.GUI.Canvas.GH_Canvas canvas)
        {
            try
            {
                var full = new Bitmap(
                    canvas.ClientRectangle.Width,
                    canvas.ClientRectangle.Height,
                    PixelFormat.Format32bppArgb);

                canvas.DrawToBitmap(full, canvas.ClientRectangle);
                return full;
            }
            catch { return null; }
        }

        /// <summary>
        /// Converts a Bitmap to a PNG byte array.
        /// </summary>
        public static byte[] ToPng(Bitmap bmp)
        {
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }
}