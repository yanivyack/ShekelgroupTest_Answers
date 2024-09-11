using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Firefly.Box;

namespace ENV.UI
{
    partial class BackwardCompatibleMessageBoxUI : Firefly.Box.UI.Form
    {
        BackwardCompatibleMessageBox _task;
        public BackwardCompatibleMessageBoxUI(BackwardCompatibleMessageBox task, string title, string message)
        {
            _task = task;
            InitializeComponent();
            this.RightToLeft = LocalizationInfo.Current.RightToLeft;
            Text = title;
            this.MessageText.Text = message;
            pictureBox1.Image = System.Drawing.SystemIcons.Warning.ToBitmap();

            _task.Yes.Value = LocalizationInfo.Current.Yes;
            _task.No.Value = LocalizationInfo.Current.No;

            btnNo.KeyPress += button1_KeyPress;
            btnYes.KeyPress += button1_KeyPress;
            btnNo.KbputText += button1_KbputText;
            btnYes.KbputText += button1_KbputText;
        }

        void button1_KbputText(char c)
        {
            ProcessKey(c);
        }

        void button1_KeyPress(object sender, KeyPressEventArgs e)
        {
            ProcessKey(e.KeyChar);
        }

        void ProcessKey(char e)
        {
            if (LocalizationInfo.Current.YesHotKeys.Contains(e.ToString()))
                Yes();
            else if (LocalizationInfo.Current.NoHotKeys.Contains(e.ToString()))
                No();
        }

        private void button2_Click(object sender, Firefly.Box.UI.Advanced.ButtonClickEventArgs e)
        {
            Yes();
        }

        void Yes()
        {
            _task.result = true;
            Close();
        }

        private void button1_Click(object sender, Firefly.Box.UI.Advanced.ButtonClickEventArgs e)
        {
            No();
        }

        void No()
        {
            Close();
        }

        public void TryFocusOnYesButton()
        {
            btnYes.TryFocus();
        }

        public void TryFocusOnNoButton()
        {
            btnNo.TryFocus();
        }
    }
    class DialogButton : Firefly.Box.UI.Button
    {
        public event Action<char> KbputText;
        public override void InputCharByCommand(char c)
        {
            if (KbputText != null)
                KbputText(c);
            base.InputCharByCommand(c);
        }
    }
    public class BackwardCompatibleMessageBox : FlowUIControllerBase
    {
        internal ENV.Data.TextColumn Yes = new Data.TextColumn() { Value = "&Yes" }, No = new Data.TextColumn() { Value = "&No" };
        bool _defaultYes = false;
        UI.BackwardCompatibleMessageBoxUI _view;
        public BackwardCompatibleMessageBox(string title, string message, bool defaultYes)
        {
            _defaultYes = defaultYes;
            AllowFindRow = false;
            Columns.Add(No, Yes);
            View = ()=>_view=new UI.BackwardCompatibleMessageBoxUI(this, title, message);
            Handlers.Add(Command.GoToPreviousControl).Invokes += e => this.result = !this.result;
        }

        protected override void OnStart()
        {
            
            if (_view != null)
            {
                if (_defaultYes)
                    _view.TryFocusOnYesButton();
                else
                    _view.TryFocusOnNoButton();
            }
        }

        internal bool result = false;

        public bool ShowDialog()
        {
            Execute();
            return result;
        }

        internal bool Show()
        {
            _view.Modal = false;
            Execute();
            return result;
        }
    }
}