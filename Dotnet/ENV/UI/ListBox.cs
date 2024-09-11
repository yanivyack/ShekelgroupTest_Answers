using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENV.Labs;
using Firefly.Box.UI;

namespace ENV.UI
{
    public partial class ListBox : Firefly.Box.UI.ListBox, ICanShowCustomHelp 
    {
        ENV.Utilities.ControlHelper _helper;
        public bool AutoExpand { get { return _helper.AutoExpand; } set { _helper.AutoExpand = value; } }
        public ListBox()
        {
            _helper = new ENV.Utilities.ControlHelper(this);
            base.Enter += () => _helper.ControlEnter(Enter); ;
            base.Leave += () => _helper.ControlLeave(Leave); ;
            base.Change += () => _helper.ControlChange(Change); ;
            base.InputValidation += () => _helper.ControlInputValidation(InputValidation); ;
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

        // Sefi: Cause the text of the items to overlap

        //public override FontScheme FontScheme
        //{
        //    get { return base.FontScheme; }

        //    set
        //    {
        //        base.FontScheme = FaceLiftDemo.MatchFontScheme(value);
        //    }
        //}
    }

    
}
