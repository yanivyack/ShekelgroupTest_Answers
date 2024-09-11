using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Firefly.Box.UI;

namespace ENV.Printing
{
    public class TextBox : Firefly.Box.UI.TextBox
    {
        public TextBox()
        {
            ColorScheme = new ColorScheme(Color.Black, Color.White);
            Style = ControlStyle.Flat;
        }

        protected override string TranslateData(string text)
        {
            if (Data != null && Data.Column == null)
            {
                if (ENV.JapaneseMethods.Enabled)
                    text = JapaneseMethods.MatchForJapaneseLength(text, Format);
            }
            return base.TranslateData(text);
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
