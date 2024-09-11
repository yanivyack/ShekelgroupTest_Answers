using System.Drawing;
using Firefly.Box.UI;
using Firefly.Box;
namespace Northwind.Views.Controls
{
    /// <summary>Close(M#7)</summary>
    [System.ComponentModel.Description("Close")]
    public partial class Close : Shared.Theme.Controls.Button 
    {
        public Close()
        {
            InitializeComponent();
        }
        void this_Click(object sender, Firefly.Box.UI.Advanced.ButtonClickEventArgs e)
        {
            e.Raise(Command.CloseForm);
        }
    }
}
