using System.Drawing;
using Firefly.Box.UI;
using Firefly.Box;
namespace Northwind.Views.Controls
{
    partial class Close
    {
        void InitializeComponent()
        {
            this.ClickEventRegistrationErasesPreviouslyRegisteredClickHandlers = true;
            this.Format = "Close";
            this.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.this_Click);
        }
    }
}
