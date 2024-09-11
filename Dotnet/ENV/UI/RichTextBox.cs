using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ENV.UI
{
    public class RichTextBox : Firefly.Box.UI.RichTextBox, ICanShowCustomHelp
    {
        ENV.Utilities.ControlHelper _helper;
        public bool AutoExpand { get { return _helper.AutoExpand; } set { _helper.AutoExpand = value; } }


        /// <summary>RichTextBox</summary>
        public RichTextBox()
        {
            _helper = new ENV.Utilities.ControlHelper(this);
            base.Enter += () => _helper.ControlEnter(Enter);
            base.Leave += () => _helper.ControlLeave(Leave);
            base.Change += () => _helper.ControlChange(Change);
            base.InputValidation += () => _helper.ControlInputValidation(InputValidation);

            InitContextMenu();

        }
        bool _setup = false;
        protected override void OnLoad()
        {
            if (_setup)
                return;
            _setup = true;
            if (LocalizationInfo.Current.RightToLeft == System.Windows.Forms.RightToLeft.Yes)
            {
                Enter+=()=>                Application.CurrentInputLanguage = InputLanguage.FromCulture(new System.Globalization.CultureInfo("HE-IL"));
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

        System.ComponentModel.IContainer _components;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (_components != null))
                _components.Dispose();
            base.Dispose(disposing);
        }

        void InitContextMenu()
        {
            _components = new System.ComponentModel.Container();
            var contextMenu = new System.Windows.Forms.ContextMenuStrip(_components);
            
            var cut = new System.Windows.Forms.ToolStripMenuItem() {Text = "Cut"};
            cut.Click += delegate { MenuManager.RaiseStatic(Firefly.Box.Command.Cut); };
            contextMenu.Items.Add(cut);

            var copy = new System.Windows.Forms.ToolStripMenuItem() { Text = "Copy" };
            copy.Click += delegate { MenuManager.RaiseStatic(Firefly.Box.Command.Copy); };
            contextMenu.Items.Add(copy);
            
            var paste = new System.Windows.Forms.ToolStripMenuItem() { Text = "Paste" };
            paste.Click += delegate { MenuManager.RaiseStatic(Firefly.Box.Command.Paste); };
            contextMenu.Items.Add(paste);

            contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

            var font = new System.Windows.Forms.ToolStripMenuItem() { Text = "Font" };
            font.Click += 
                delegate
                    {
                        var fontDialog = new System.Windows.Forms.FontDialog() {Font = this.SelectionFont};
                        if (fontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            this.SelectionFont = fontDialog.Font;
                    };
            contextMenu.Items.Add(font);

            var color = new System.Windows.Forms.ToolStripMenuItem() { Text = "Color" };
            color.Click +=
                delegate
                {
                    var fontDialog = new System.Windows.Forms.ColorDialog() { Color = this.SelectionColor };
                    if (fontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        this.SelectionColor = fontDialog.Color;
                };
            contextMenu.Items.Add(color);


            contextMenu.Opened +=
                (sender, args) =>
                    {
                        var cutEnabled = cut.Enabled;
                        var copyEnabled = copy.Enabled;
                        var pasteEnabled = paste.Enabled;
                        ENV.Common.RunOnLogicContext(this, () =>
                        {
                            cutEnabled = Firefly.Box.Command.Cut.Enabled;
                            copyEnabled = Firefly.Box.Command.Copy.Enabled;
                            pasteEnabled = Firefly.Box.Command.Paste.Enabled;
                        });
                        cut.Enabled = cutEnabled;
                        copy.Enabled = copyEnabled;
                        paste.Enabled = pasteEnabled;
                    };

            _defaultContextMenu = contextMenu;
        }

        ContextMenuStrip _defaultContextMenu;
        public override ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return base.ContextMenuStrip ?? _defaultContextMenu;
            }

            set
            {
                base.ContextMenuStrip = value;
            }
        }
    }
}
