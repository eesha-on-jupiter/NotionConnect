using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace NotionConnect
{
    /// <summary>
    /// Base class for any Notion component that needs a clickable button.
    /// Subclasses override ButtonLabel and OnButtonPressed to provide
    /// their own label text and trigger logic.
    /// </summary>
    public abstract class ButtonComponent : GH_Component
    {
        private bool _triggered = false;

        protected ButtonComponent(string name, string nickname, string description, string category, string subcategory)
            : base(name, nickname, description, category, subcategory)
        { }

        /// <summary>Text shown on the button.</summary>
        public abstract string ButtonLabel { get; }

        /// <summary>
        /// True during the SolveInstance call immediately after the button was pressed.
        /// Resets to false automatically after SolveInstance completes.
        /// </summary>
        protected bool IsTriggered => _triggered;

        /// <summary>Call base.AfterSolveInstance() if you override this.</summary>
        protected override void AfterSolveInstance()
        {
            _triggered = false;
            base.AfterSolveInstance();
        }

        // ---- Button trigger ----

        public void TriggerButton()
        {
            _triggered = true;
            ExpireSolution(true);
        }

        // ---- Custom attributes ----

        public override void CreateAttributes()
        {
            m_attributes = new ButtonAttributes(this);
        }
    }


    // ---- Reusable button attributes ----

    public class ButtonAttributes : GH_ComponentAttributes
    {
        private RectangleF _buttonBounds;

        private ButtonComponent ButtonOwner => (ButtonComponent)Owner;

        public ButtonAttributes(ButtonComponent owner) : base(owner) { }

        protected override void Layout()
        {
            base.Layout();

            float padding = 8f;
            float buttonH = 20f;
            float bottomMargin = 6f;

            var expanded = Bounds;
            expanded.Height += buttonH + bottomMargin * 2;
            Bounds = expanded;

            _buttonBounds = new RectangleF(
                Bounds.Left + padding,
                Bounds.Bottom - buttonH - bottomMargin,
                Bounds.Width - padding * 2,
                buttonH
            );
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if (channel == GH_CanvasChannel.Objects)
            {
                var fillColor = Color.FromArgb(210, 38, 38, 38);
                var borderColor = Color.FromArgb(255, 95, 95, 95);
                var textColor = Color.FromArgb(255, 185, 185, 185);

                using (var brush = new SolidBrush(fillColor))
                using (var pen = new Pen(borderColor, 0.8f))
                using (var tBrush = new SolidBrush(textColor))
                using (var font = new Font("Arial", 6f, FontStyle.Regular))
                {
                    var rect = Rectangle.Round(_buttonBounds);

                    using (var path = RoundedRect(rect, 4))
                    {
                        graphics.FillPath(brush, path);
                        graphics.DrawPath(pen, path);
                    }

                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    };

                    graphics.DrawString(ButtonOwner.ButtonLabel, font, tBrush, _buttonBounds, sf);
                }
            }
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left && _buttonBounds.Contains(e.CanvasLocation))
            {
                ButtonOwner.TriggerButton();
                return GH_ObjectResponse.Handled;
            }
            return base.RespondToMouseDown(sender, e);
        }

        private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle rect, int radius)
        {
            int d = radius * 2;
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}