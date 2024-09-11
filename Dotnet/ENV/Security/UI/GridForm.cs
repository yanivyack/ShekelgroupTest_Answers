using System.Drawing;
using Firefly.Box;
using Firefly.Box.UI.Advanced;

namespace ENV.Security.UI
{

    /// <summary>GridForm</summary>
    public partial class GridForm : Form
    {
        internal static CustomCommand CancelCommand = new CustomCommand
        {
            Precondition = CustomCommandPrecondition.LeaveRow
        };

        internal static CustomCommand OkCommand = new CustomCommand();

        /// <summary>GridForm</summary>
        public GridForm()
        {
            InitializeComponent();
            this.okBtn.Text = LocalizationInfo.Current.Ok;
            this.cancelBtn.Text = LocalizationInfo.Current.Cancel;
        }

        protected int _bottomOfset = 45;

        public int BottomOfset
        {
            get { return _bottomOfset; }
            set { _bottomOfset = value; }
        }

        protected override void OnLayout(System.Windows.Forms.LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            {
                if (grid1 != null)
                    grid1.Bounds = new Rectangle(5, 15, ClientSize.Width - 10, ClientSize.Height - 5 - _bottomOfset);
                if (okBtn != null)
                {
                    okBtn.Location = new Point(okBtn.Left = grid1.Right - okBtn.Width, this.ClientSize.Height - 5 - okBtn.Height);
                }
                if (cancelBtn != null)
                {
                    cancelBtn.Location = new Point(cancelBtn.Left = okBtn.Left - cancelBtn.Width - 6, this.ClientSize.Height - 5 - cancelBtn.Height);
                }
            }
        }

        private void okBtn_Click(object sender, ButtonClickEventArgs e)
        {
            e.Raise(OkCommand);
        }

        private void cancelBtn_Click(object sender, ButtonClickEventArgs e)
        {
            e.Raise(CancelCommand);
        }
    }
}
