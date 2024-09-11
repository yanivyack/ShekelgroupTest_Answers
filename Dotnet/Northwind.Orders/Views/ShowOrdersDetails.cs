using Firefly.Box.UI;
using System.Drawing;
using Firefly.Box.UI.Advanced;
using Firefly.Box;
using Northwind.Shared.Theme;
namespace Northwind.Orders.Views
{
    /// <summary>Details(P#5.1)</summary>
    partial class ShowOrdersDetails : Shared.Theme.Controls.CompatibleForm 
    {
        ShowOrders.Details _controller;
        internal ShowOrdersDetails(ShowOrders.Details controller)
        {
            _controller = controller;
            InitializeComponent();
        }
        void cboOrder_Details_ProductID_BindListSource(object sender, System.EventArgs e)
        {
            var Products_ = new Models.Products();
            cboOrder_Details_ProductID.ListSource = Products_;
            cboOrder_Details_ProductID.ValueColumn = Products_.ProductID;
            cboOrder_Details_ProductID.DisplayColumn = Products_.ProductName;
            cboOrder_Details_ProductID.ListOrderBy = Products_.SortByProductName;
        }
    }
}
