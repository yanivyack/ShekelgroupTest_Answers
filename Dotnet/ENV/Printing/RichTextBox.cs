using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Firefly.Box.UI;

namespace ENV.Printing
{
    public class RichTextBox:Firefly.Box.UI.RichTextBox
    {
        public RichTextBox()
        {
            Style = Firefly.Box.UI.ControlStyle.Flat;
            ColorScheme = new ColorScheme(Color.Black, Color.White);
        }
        protected override string Translate(string term)
        {
            return base.Translate(ENV.Languages.Translate(term));
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
