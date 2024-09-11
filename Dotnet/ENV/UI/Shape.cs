using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENV.Labs;
using Firefly.Box.UI;

namespace ENV.UI
{
    public class Shape : Firefly.Box.UI.Shape
    {
        protected override string Translate(string term)
        {
            return base.Translate(ENV.Languages.Translate(term));
        }
        protected override void OnLoad()
        {
            if (FaceLiftDemo.Enabled)
            {
                if (Style != Firefly.Box.UI.ControlStyle.Flat || BackColor == System.Drawing.SystemColors.ButtonFace ||
                    BackColor.Equals(System.Drawing.Color.FromArgb(255, 192, 192, 192)))
                {
                    if (Style != Firefly.Box.UI.ControlStyle.Flat)
                        Style = Firefly.Box.UI.ControlStyle.Standard;
                    FixedBackColorInNonFlatStyles = false;
                    BackColor = FaceLiftDemo.BackGroundColor;
                }
            }
            base.OnLoad();
            if (DisableDrop)
                AllowDrop = false;
        }
        bool _hadMissingColor = false;
        public override ColorScheme ColorScheme
        {
            get { return base.ColorScheme; }
            set
            {
                base.ColorScheme = value;
                if (_hadMissingColor)
                    BorderColor = value.ForeColor;
                if (value is MissingColor)
                {
                    BorderColor = value.BackColor;
                    _hadMissingColor = true;
                }
            }
        }

        public override FontScheme FontScheme
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

        public bool DisableDrop { get; set; }

        public override bool AllowDrop
        {
            get { return base.AllowDrop; }
            set { base.AllowDrop = DisableDrop ? false : value; }
        }
    }
    public class MissingColor : Firefly.Box.UI.ColorScheme
    {
        public MissingColor() : base(System.Drawing.SystemColors.ControlText, System.Drawing.SystemColors.Control) {

        }
    }
}
