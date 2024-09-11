using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ENV.UI
{
    public class TreeView : Firefly.Box.UI.TreeView 
    {
        public TreeView()
        {
            _helper = new ENV.Utilities.ControlHelper(this);
            base.Enter += () => _helper.ControlEnter(Enter);
            base.Leave += () => _helper.ControlLeave(Leave);
            base.Change += () => _helper.ControlChange(Change);
            base.InputValidation += () => _helper.ControlInputValidation(InputValidation);
            KeepExpandedNodesStateAfterExit = false;
        }
        protected override System.Windows.Forms.ImageList GetImageList(string imageListLocation)
        {
            return ENV.Common.GetImageList(imageListLocation);
        }
        public void UseDefaultImageList()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TreeView));
            System.Windows.Forms.ImageList imageList1 = new System.Windows.Forms.ImageList();
            imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            imageList1.TransparentColor = System.Drawing.Color.Fuchsia;
            ImageList = imageList1;
        }
        
        ENV.Utilities.ControlHelper _helper;
        /*
        [DefaultValue(false)]
        public bool AutoExpand { get { return _helper.AutoExpand; } set { _helper.AutoExpand = value; } }
        */
        public void ClearExpandEvent()
        {
            _helper.ClearExpandEvent();
        }
        protected T Create<T>()
        {
            return AbstractFactory.Create<T>();
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
    }

}
