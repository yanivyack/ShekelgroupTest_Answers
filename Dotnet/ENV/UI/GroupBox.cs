using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ENV.Labs;
using Firefly.Box.UI;

namespace ENV.UI
{
    public class GroupBox : Firefly.Box.UI.GroupBox
    {
        protected override string Translate(string term)
        {
            return base.Translate(ENV.Languages.Translate(term));
        }
        protected override void OnLoad()
        {

            if (FaceLiftDemo.Enabled)
            {
                FixedBackColorInNonFlatStyles = false;
                Style = ControlStyle.Standard;
            }
            base.OnLoad();
            if (DisableDrop)
                AllowDrop = false;
            if (DisableSunkenAndRaisedStyles && (Style == ControlStyle.Raised || Style == ControlStyle.Sunken))
                Style = ControlStyle.Standard;
        }
        public bool DisableSunkenAndRaisedStyles { get; set; }
       
       
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

        public override bool ForceTransparentBackgroundOnStandardStyle
        {
            get { return base.ForceTransparentBackgroundOnStandardStyle; }
            set
            {
                if (DesignMode)
                    base.ForceTransparentBackgroundOnStandardStyle = value;
                else base.ForceTransparentBackgroundOnStandardStyle = value && UseVisualStyles;
            }
        }

        public bool DisableDrop { get; set; }

        public override bool AllowDrop
        {
            get { return base.AllowDrop; }
            set { base.AllowDrop = DisableDrop ? false : value; }
        }
    }
}
