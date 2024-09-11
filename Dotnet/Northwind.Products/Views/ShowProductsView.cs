using Firefly.Box.UI;
using System.Drawing;
using Firefly.Box.UI.Advanced;
using Firefly.Box;
using Northwind.Shared.Theme;
namespace Northwind.Products.Views
{
    /// <summary>Show Products(P#4)</summary>
    partial class ShowProductsView : Shared.Theme.Controls.CompatibleForm 
    {
        ShowProducts _controller;
        internal ShowProductsView(ShowProducts controller)
        {
            _controller = controller;
            InitializeComponent();
        }
    }
}
