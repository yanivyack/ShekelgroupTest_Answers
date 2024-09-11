using Firefly.Box.UI;
using System.Drawing;
using Firefly.Box.UI.Advanced;
using Firefly.Box;
using Northwind.Shared.Theme;
namespace Northwind.Customers.Views
{
    /// <summary>Show Customers(P#3)</summary>
    partial class ShowCustomersView : Shared.Theme.Controls.CompatibleForm 
    {
        ShowCustomers _controller;
        internal ShowCustomersView(ShowCustomers controller)
        {
            _controller = controller;
            InitializeComponent();
        }
    }
}
