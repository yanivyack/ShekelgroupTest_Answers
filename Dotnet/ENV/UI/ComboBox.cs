using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ENV.Labs;
using Firefly.Box.UI;

namespace ENV.UI
{
    public partial class ComboBox : Firefly.Box.UI.ComboBox, ICanShowCustomHelp
    {
        ENV.Utilities.ControlHelper _helper;
        public bool AutoExpand { get { return _helper.AutoExpand; } set { _helper.AutoExpand = value; } }


        /// <summary>ComboBox</summary>
        public ComboBox()
        {
            InitializeComponent();
            _helper = new ENV.Utilities.ControlHelper(this);
            base.Enter += () => _helper.ControlEnter(Enter);
            base.Leave += () => _helper.ControlLeave(Leave);
            base.Change += () => _helper.ControlChange(Change);
            base.InputValidation += () => _helper.ControlInputValidation(InputValidation);

            //HideSelectionBoxWhileInactiveOnGrid = true;
        }
        bool _autoHeightOnStandardStyle;
        [System.ComponentModel.DefaultValue(false)]
        public bool AutoHeightOnStandardStyle
        {
            get { return _autoHeightOnStandardStyle; }
            set
            {

                _autoHeightOnStandardStyle = value;
                RecalcStyle();
                RecalcHeight();
            }
        }
        protected T Create<T>()
        {
            return AbstractFactory.Create<T>();
        }
        private bool ShouldSerializeDrawMode()
        {
            return !DrawModeByStyle;
        }
        public override DrawMode DrawMode
        {
            get
            {
                return base.DrawMode;
            }

            set
            {
                base.DrawMode = value;
            }
        }

        /// <summary>
        /// When set to true - Non flat style combos will be drawen using DrawMode Normal
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool DrawModeByStyle
        {
            get { return _drawModeByStyle; }
            set { _drawModeByStyle = value; RecalcStyle(); }
        }
        void RecalcStyle()
        {
            if (SuppressSunkenStyle)
                if (Style == Firefly.Box.UI.ControlStyle.Sunken)
                    Style = Firefly.Box.UI.ControlStyle.Standard;
            if (DrawModeByStyle)
            {
                if (Style != Firefly.Box.UI.ControlStyle.Flat)
                {
                    if (DrawMode != DrawMode.Normal)
                        DrawMode = System.Windows.Forms.DrawMode.Normal;
                }
                else if (DrawMode != DrawMode.OwnerDrawVariable)
                    DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;

            }

        }
        bool _drawModeByStyle;
        /// <summary>
        /// Determines if the Sunken style should be replaced by Standard Style.
        /// </summary>

        [System.ComponentModel.DefaultValue(false)]
        public bool SuppressSunkenStyle
        {
            get
            {
                return _suppressSunkenStyle;
            }
            set
            {
                _suppressSunkenStyle = value;
                RecalcStyle();
            }
        }
        bool _suppressSunkenStyle;
        protected override void OnLoad()
        {

            if (Labs.FaceLiftDemo.Enabled)
                AutoCompleteAPLHA = true;
            base.OnLoad();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RecalcHeight();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            RecalcHeight();
        }
        bool _inProgress = false;
        void RecalcHeight()
        {
            if (Parent == null) return;
            if (_inProgress)
                return;
            _inProgress = true;
            try
            {
                var p = Parent;
                while (p != null && !(p is System.Windows.Forms.Form))
                {
                    if (p is Grid || p is GridColumn) return;
                    p = p.Parent;
                }

                if (AutoHeightOnStandardStyle && Style == ControlStyle.Standard)
                {
                    var preferredHeight = PreferredHeight;
                    if (Height < preferredHeight) Height = preferredHeight;
                }
            }
            finally
            {
                _inProgress = false;
            }
        }
        public override ControlStyle Style
        {
            get
            {
                return base.Style;
            }

            set
            {
                base.Style = value;
                RecalcStyle();
                RecalcHeight();

            }
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

    }
}
