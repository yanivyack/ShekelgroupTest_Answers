using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ENV.Data;
using Firefly.Box.UI;

namespace ENV.UI
{
    public partial class RadioButton:Firefly.Box.UI.RadioButton,ICanShowCustomHelp
    {
         ENV.Utilities.ControlHelper _helper;
        public bool AutoExpand { get { return _helper.AutoExpand; } set { _helper.AutoExpand = value; } }


        /// <summary>CheckBox</summary>
        public RadioButton()
        {
           
            _helper = new ENV.Utilities.ControlHelper(this);
            base.Enter += () => _helper.ControlEnter(Enter); 
            base.Leave += () => _helper.ControlLeave(Leave); 
            base.Change += () => _helper.ControlChange(Change); 
            base.InputValidation += () => _helper.ControlInputValidation(InputValidation); 
        }
        protected override string Translate(string term)
        {
            return base.Translate(ENV.Languages.Translate(term));
        }
        public void ClearExpandEvent()
        {
            _helper.ClearExpandEvent();
        }
        protected T Create<T>()
        {
            return AbstractFactory.Create<T>();
        }
        protected override void OnLoad()
        {
            if (ENV.Labs.FaceLiftDemo.Enabled)
            {
                if (Parent is Form)
                {
                    FixedBackColorInNonFlatStyles = false;
                    BackColor = ENV.Labs.FaceLiftDemo.BackGroundColor;
                }
            }
          
            if (UserSettings.Version8Compatible && !((Data.Column is TextColumn) && Data.Column.Format.Contains("H")))
                RightToLeftText = RightToLeft.No;
            base.OnLoad();
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
                base.FontScheme = Labs.FaceLiftDemo.MatchFontScheme(value);
            }
        }

        protected override System.Drawing.Image GetImage(string imageLocation)
        {
            return ENV.Common.GetImage(imageLocation);
        }

    }
}
