using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ENV.UI
{
    public partial class CheckBox : Firefly.Box.UI.CheckBox, ICanShowCustomHelp
    {
        ENV.Utilities.ControlHelper _helper;
        public bool AutoExpand { get { return _helper.AutoExpand; } set { _helper.AutoExpand = value; } }


        /// <summary>CheckBox</summary>
        public CheckBox()
        {


            _helper = new ENV.Utilities.ControlHelper(this);
            base.Enter += () => _helper.ControlEnter(Enter);
            base.Leave += () => _helper.ControlLeave(Leave);
            base.Change += () => _helper.ControlChange(Change);
            base.InputValidation += () => _helper.ControlInputValidation(InputValidation);
            UseColumnInputRangeWhenEmptyText = true;
        }
        protected override string Translate(string term)
        {
            return base.Translate(ENV.Languages.Translate(term));
        }
        public void ClearExpandEvent()
        {
            _helper.ClearExpandEvent();
        }

        public override event System.Action Enter;
        public override event System.Action Leave;
        public override event System.Action Change;
        public override event System.Action InputValidation;
        public new event System.Action Expand { add { _helper.Expand += value; } remove { _helper.Expand -= value; } }
        /// <summary>
        /// Only used for the UserMethods.ControlSelectProgram method - backward compatability only
        /// </summary>
        public Type ExpandClassType { set { _helper.ExpandClassType = value; } get { return _helper.ExpandClassType; } }
        public AfterExpandGoToNextControlOptions AfterExpandGoToNextControl { get { return _helper.AfterExpandGoToNextControl; } set { _helper.AfterExpandGoToNextControl = value; } }
        public string StatusTip { get { return _helper.StatusTip; } set { _helper.StatusTip = value; } }
        public CustomHelp CustomHelp { get; set; }
        protected T Create<T>()
        {
            return AbstractFactory.Create<T>();
        }

        int _smallCheckBoxWidth = 10;
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);
            _smallCheckBoxWidth = (int)(_smallCheckBoxWidth * factor.Width);
        }
        public bool SmallCheckBoxOnFlatStyle { get; set; }
        protected override void OnLoad()
        {
            if (SmallCheckBoxOnFlatStyle && this.Style == Firefly.Box.UI.ControlStyle.Flat)
            {
                CheckBoxPadding = true;
                MaxCheckBoxWidth = _smallCheckBoxWidth;
            }
            if (ENV.Labs.FaceLiftDemo.Enabled)
            {
                if (Parent is Form)
                {
                    FixedBackColorInNonFlatStyles = false;
                    BackColor = ENV.Labs.FaceLiftDemo.BackGroundColor;
                    if (BackColor.ToArgb() == ForeColor.ToArgb())
                    {
                        if (BackColor.ToArgb() == Color.White.ToArgb())
                            ForeColor = SystemColors.ControlText;
                    }
                }
            }
            base.OnLoad();
        }
        Firefly.Box.UI.ColorScheme _origColor;
        public override Firefly.Box.UI.ColorScheme ColorScheme
        {
            get
            {
                return base.ColorScheme;
            }
            set
            {
                _origColor = value;
                if (FixedForeColor && value != null)
                {
                    BorderColor = value.ForeColor;
                    value = new Firefly.Box.UI.ColorScheme(SystemColors.ControlText, value.BackColor);
                }
                base.ColorScheme = value;
            }
        }
        bool _fixedForColor;
        public bool FixedForeColor
        {
            get { return _fixedForColor; }
            set
            {
                _fixedForColor = value;
                ColorScheme = _origColor;
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
                base.FontScheme = Labs.FaceLiftDemo.MatchFontScheme(value);
            }
        }
    }
}
