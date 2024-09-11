using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ENV.Labs;
using Firefly.Box.UI;

namespace ENV.UI
{
    public class Label : Firefly.Box.UI.Label
    {
        public Label()
        {

        }
        protected override string Translate(string term)
        {
            return base.Translate(ENV.Languages.Translate(term));
        }
        public override Firefly.Box.UI.ColorScheme ColorScheme
        {
            get
            {
                return base.ColorScheme;
            }
            set
            {


                base.ColorScheme = FaceLiftDemo.MatchColorScheme(value);
            }
        }
        public override Firefly.Box.UI.FontScheme FontScheme
        {
            get
            {
                return base.FontScheme;
            }
            set
            {
                base.FontScheme = FaceLiftDemo.MatchFontScheme(value);
            }
        }
        protected override void OnLoad()
        {
            base.OnLoad();
            if (UseRtf)
                FixedBackColorInNonFlatStyles = false;
            if (DisableDrop)
                AllowDrop = false;
        }

        public bool DisableDrop { get; set; }

        public override bool AllowDrop
        {
            get { return base.AllowDrop; }
            set { base.AllowDrop = DisableDrop ? false : value; }
        }
        ColorScheme _borderColorScheme;
        public ColorScheme BorderColorScheme
        {
            get { return _borderColorScheme; }
            set
            {
                _borderColorScheme = value;
                if (value != null)
                    BorderColor = value.ForeColor;
            }
        }
        bool ShouldSerializeBorderColor()
        {
            if (_borderColorScheme == null)
                return BorderColor != ForeColor;
            return BorderColor != _borderColorScheme.ForeColor;
        }
        public override Color BorderColor
        {
            get
            {
                return base.BorderColor;
            }

            set
            {
                base.BorderColor = value;
            }
        }
    }
}
