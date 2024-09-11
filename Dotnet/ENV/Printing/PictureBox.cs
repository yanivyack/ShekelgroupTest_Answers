using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Firefly.Box.UI;

namespace ENV.Printing
{
    public class PictureBox:Firefly.Box.UI.PictureBox
    {
        public PictureBox()
        {
            Style = Firefly.Box.UI.ControlStyle.Flat;
            ColorScheme = new ColorScheme(Color.Black, Color.White);
        }

        protected override string Translate(string term)
        {
            return base.Translate(ENV.Languages.Translate(term));
        }
        protected override System.Drawing.Image GetImage(string imageLocation)
        {
            return ENV.Common.GetImage(imageLocation);
        }
        public override FontScheme FontScheme
        {
            get
            {
                return base.FontScheme;
            }

            set
            {
                base.FontScheme = ENV.UI.LoadableFontScheme.GetPrintingFont(value);
            }
        }

    }
}
