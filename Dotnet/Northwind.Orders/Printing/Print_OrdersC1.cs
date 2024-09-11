using Firefly.Box.UI;
using System.Drawing;
using Firefly.Box.UI.Advanced;
using Firefly.Box;
using Northwind.Shared.Theme;
namespace Northwind.Orders.Printing
{
    /// <summary>Print - Orders(P#7)</summary>
    partial class Print_OrdersC1 : Shared.Theme.Printing.ReportLayout 
    {
        Print_Orders _controller;
        internal Print_OrdersC1(Print_Orders controller) : base(controller)
        {
            _controller = controller;
            InitializeComponent();
        }
    }
}
