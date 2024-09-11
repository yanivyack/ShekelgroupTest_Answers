using ENV;
using Firefly.Box;
using System;
namespace Northwind.Views
{
    partial class ApplicationMdi : System.Windows.Forms.Form,IHaveAMenu 
    {
        MenuManager _menuManager;
        public override System.Drawing.Size MinimumSize { get { return Common.GetMDIMinimumSize(this, base.MinimumSize); } set { base.MinimumSize = value; } }
        internal System.Windows.Forms.ContextMenuStrip _optionsContextMenuStrip;
        public ApplicationMdi()
        {
            Icon = Common.DefaultIcon;
            InitializeComponent();
            _menuManager = new MenuManager(this, new ContextMenuMap(components));
            Common.MDILoad(this);
            new ENV.UI.Menus.DeveloperToolsMenu(Application.Instance, typeof (Shared.DataSources)).AddToContextMenu(_menuManager, _optionsContextMenuStrip);
        }
        public void DoOnMenu(Action<MenuManager> action)
        {
            action(_menuManager);
        }
        [System.Diagnostics.DebuggerStepThrough]
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            Common.ProcessMDIMessage(this, m);
            base.WndProc(ref m);
            Common.ProcessMDIMessageAfterMDI(this, m, SizeFromClientSize);
        }
        protected override void OnClosed(EventArgs e)
        {
            Common.MDIClose(this);
            base.OnClosed(e);
        }
        protected override void ScaleControl(System.Drawing.SizeF factor, System.Windows.Forms.BoundsSpecified specified)
        {
            Common.MDIScale(this, factor);
            base.ScaleControl(factor, specified);
        }
        void expandStatusLabel_Click(object sender, EventArgs e)
        {
            _menuManager.RunOnActiveContext(() => _menuManager.Raise(Command.Expand));
        }
        void expandTextBoxStatusLabel_Click(object sender, EventArgs e)
        {
            _menuManager.RunOnActiveContext(() => _menuManager.Raise(Command.ExpandTextBox));
        }
        protected T Create<T>()
        {
            return AbstractFactory.Create<T>();
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Grid().Run();
        }
    }
}
