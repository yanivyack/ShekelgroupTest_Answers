using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ENV.UI
{
    public class PictureBox:Firefly.Box.UI.PictureBox,ICanShowCustomHelp
    {
         ENV.Utilities.ControlHelper _helper;
        public bool AutoExpand { get { return _helper.AutoExpand; } set { _helper.AutoExpand = value; } }


        /// <summary>CheckBox</summary>
        public PictureBox()
        {
            _helper = new ENV.Utilities.ControlHelper(this);
            base.Enter += () =>
            {
                if (!ReadOnly && !ENV.UserMethods.Instance.Stat(0, "Q") && Data.Column != null)
                {
                    var x = new OpenFileDialog();

                    x.Filter = "All Image Files|*.BMP;*.JPG;*.JPEG;*.PNG;*.ico;*.pcx;*.tif;*.png|All Files|*.*";
                    try
                    {
                        var p = ((Firefly.Box.Data.TextColumn)Data.Column).TrimEnd();
                        if (!Firefly.Box.Text.IsNullOrEmpty(p))
                        {
                            x.FileName = System.IO.Path.GetFileName(p);
                            x.InitialDirectory = System.IO.Path.GetDirectoryName(p);
                        }


                    }
                    catch
                    {
                    }
                    if (x.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            var tc = Data.Column as ENV.Data.TextColumn;
                            if (tc != null)
                                tc.Value = x.FileName;
                            else {
                                var bac = Data.Column as ENV.Data.ByteArrayColumn;
                                if (bac != null)
                                    bac.Value = System.IO.File.ReadAllBytes(x.FileName);
                            }
                        }
                        catch
                        {
                        }
                    }
                    Common.Raise(Firefly.Box.Command.GoToNextControl);
                }
                _helper.ControlEnter(Enter);
            };
            base.Leave += () => _helper.ControlLeave(Leave); 
            base.Change += () => _helper.ControlChange(Change); 
            base.InputValidation += () => _helper.ControlInputValidation(InputValidation); 
        }
        protected T Create<T>()
        {
            return AbstractFactory.Create<T>();
        }
        protected override string Translate(string term)
        {
            return base.Translate(ENV.Languages.Translate(term));
        }
        protected override System.Drawing.Image GetImage(string imageLocation)
        {
            return ENV.Common.GetImage(imageLocation, 
                ImageLayout == Firefly.Box.UI.ImageLayout.None || ImageLayout == Firefly.Box.UI.ImageLayout.Tile ? System.Drawing.Size.Empty : Size);
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
    }
}
