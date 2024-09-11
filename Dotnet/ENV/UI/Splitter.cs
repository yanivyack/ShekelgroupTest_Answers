using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ENV.UI
{
    public class Splitter:System.Windows.Forms.Splitter
    {
        public bool Border3D;
        public Splitter()
        {
            Border3D = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!Border3D)
            {
                base.OnPaint(e);
                return;
            }

            e.Graphics.FillRectangle(SystemBrushes.Control, ClientRectangle);
            ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, Border3DStyle.Raised);
        }
        public int ZOrder { get; set; }

        protected override Cursor DefaultCursor
        {
            get
            {
                switch (this.Dock)
                {
                    case DockStyle.Top:
                    case DockStyle.Bottom:
                        return Cursors.SizeNS;
                    case DockStyle.Left:
                    case DockStyle.Right:
                        return Cursors.SizeWE;
                    default:
                        return base.DefaultCursor;
                }
            }
        }
    }
}
