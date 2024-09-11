using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firefly.Box.UI;
using Firefly.Box;

namespace ENV.UI
{
    [ToolboxBitmap(typeof(System.Windows.Forms.HelpProvider))]
    [System.ComponentModel.TypeDescriptionProvider(typeof(Firefly.Box.UI.Advanced.ValueInheritanceTypeDescriptionProvider))]
    public class CustomHelp : IComponent
    {
        public CustomHelp()
        {
            Size = new Size(300, 300);
            Border = ControlBorderStyle.Thick;
            TitleBar = true;
            SystemMenu = true;
        }

        public event EventHandler Disposed;
        static ContextStatic<System.Windows.Forms.Form> _formForHelp = new ContextStatic<System.Windows.Forms.Form>(() => new System.Windows.Forms.Form());

        public void Show()
        {
            if (!string.IsNullOrWhiteSpace(HelpFileName))
            {
                var fn = PathDecoder.DecodePath(HelpFileName);
                if (System.IO.File.Exists(fn))
                {
                    Firefly.Box.Context.Current.InvokeUICommand(() =>
                     System.Windows.Forms.Help.ShowHelp(_formForHelp.Value, fn, HelpNavigator, HelpKeyword));
                }
                return;
            }

            var f = new ENV.UI.Form
            {
                Location = Location,
                Size = Size,
                FontScheme = FontScheme,
                ColorScheme = ColorScheme,
                Border = Border,
                MinimizeBox = false,
                MaximizeBox = false,
                Text = Caption,
                TitleBar = TitleBar,
                SystemMenu = SystemMenu
            };
            var tc = new ENV.Data.TextColumn() { DefaultValue = Text };
            var label = new ENV.UI.TextBox
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                Multiline = true,
                FontScheme = FontScheme,
                ColorScheme = ColorScheme,
                Data = tc,
                ReadOnly = true,
                Style = ControlStyle.Flat,
                Alignment = ContentAlignment.TopLeft,
                RightToLeftLayout = true
            };
            f.Controls.Add(label);
            var uic = new Firefly.Box.UIController
            {
                Title = "InternalHelp",
                View = f,
                AllowUpdate = false,
                AllowDelete = false,
                AllowActivitySwitch = false,
                AllowIncrementalSearch = false,
                AllowInsert = false,
                AllowSelect = false,
                AllowInsertInUpdateActivity = false,
                Activity = Firefly.Box.Activities.Browse
            };
            uic.Run();
        }

        public void Dispose()
        {
            if (Disposed != null)
                Disposed(this, new EventArgs());
        }

        public string Text { get; set; }
        public Size Size { get; set; }
        public Point Location { get; set; }
        public FontScheme FontScheme { get; set; }
        public ColorScheme ColorScheme { get; set; }
        public ControlBorderStyle Border { get; set; }
        public bool TitleBar { get; set; }
        public bool SystemMenu { get; set; }
        public string Caption { get; set; }

        public ISite Site { get; set; }

        public string HelpFileName { get; set; }
        HelpNavigator _helpNavigator = HelpNavigator.TopicId;
        public HelpNavigator HelpNavigator { get { return _helpNavigator; } set { _helpNavigator = value; } }
        public string HelpKeyword { get; set; }

    }
    interface ICanShowCustomHelp
    {
        CustomHelp CustomHelp { get; set; }
        System.Type ExpandClassType { get; set; }

    }
}