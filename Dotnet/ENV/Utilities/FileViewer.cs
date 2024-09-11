using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ENV.Utilities
{
    public class FileViewer : UIControllerBase
    {
        Data.TextColumn Content = new ENV.Data.TextColumn();
        Firefly.Box.Text _originalValue, _currentValue;
        ENV.UI.TextBox tc;
        readonly string _fileName;
        internal static Func< ENV.UI.TextBox> TextBoxFactory = () => new ENV.UI.TextBox();
        public FileViewer(string fileName, Firefly.Box.UI.Form form)
        {
            _fileName = PathDecoder.DecodePath(fileName);

            tc = TextBoxFactory();
            tc.Dock = System.Windows.Forms.DockStyle.Fill;
            tc.ScrollBars = true;
            tc.AllowVerticalScroll = true;
            tc.AllowHorizontalScroll = true;
            tc.WordWrap = false;
            tc.Multiline = true;
            tc.AcceptsReturn = true;
            tc.RightToLeftLayout = false;
            tc.RightToLeftByFormat = false;
            tc.InputLanguageByRightToLeft = true;

            tc.Data = Content;

            ((AbstractUIController) this).View = () =>
                                                     {
                                                         var f = new ENV.UI.Form();
                                                         // Form.FitToMDI = true;
                                                         f.Controls.Add(tc);
                                                         ConfirmUpdate = true;
                                                         f.Text = _fileName;
                                                         if (form==null)
                                                             f.FitToMDI = true;
                                                         return f;
                                                     };
                
            try
            {
                _originalValue = System.IO.File.ReadAllText(_fileName, LocalizationInfo.Current.OuterEncoding);
            }
            catch
            {
            }
            Content.Value = _originalValue;
            AllowDelete = false;
            AllowInsert = false;
            AllowBrowse = false;
            if (form != null)
            {
                BindView(form);
                tc.FontScheme = form.FontScheme;
            }
            RightToLeft = LocalizationInfo.Current.RightToLeft == System.Windows.Forms.RightToLeft.Yes;

        }
        public bool RightToLeft
        {
            set
            {

                {

                    tc.RightToLeft = value ? System.Windows.Forms.RightToLeft.Yes : System.Windows.Forms.RightToLeft.No;
                    tc.Alignment = value ? System.Drawing.ContentAlignment.TopRight : System.Drawing.ContentAlignment.TopLeft;
                }
            }
        }
        protected override void OnStart()
        {
            Raise(Firefly.Box.Command.PlaceCursorAtStartOfTextBox);
        }
        protected override void OnSavingRow()
        {
            _currentValue = Content.Value.TrimEnd();
        }
        public void View()
        {
            tc.ReadOnly = true;
            Execute();
        }
        public void Edit()
        {
            tc.ReadOnly = false;
            Execute();
            if (_originalValue != _currentValue && _currentValue != null)
                try
                {
                    System.IO.File.WriteAllText(_fileName, _currentValue, LocalizationInfo.Current.OuterEncoding);

                }
                catch
                {
                }
        }

    }
}
