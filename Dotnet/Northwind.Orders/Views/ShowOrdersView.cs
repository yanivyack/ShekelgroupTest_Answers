using Firefly.Box.UI;
using System.Drawing;
using Firefly.Box.UI.Advanced;
using Firefly.Box;
using Northwind.Shared.Theme;
namespace Northwind.Orders.Views
{
    /// <summary>Show Orders(P#5)</summary>
    partial class ShowOrdersView : Shared.Theme.Controls.CompatibleForm 
    {
        ShowOrders _controller;
        internal ShowOrdersView(ShowOrders controller)
        {
            _controller = controller;
            InitializeComponent();
        }
        void cboOrders_CustomerID_BindListSource(object sender, System.EventArgs e)
        {
            var Customers_ = new Models.Customers();
            cboOrders_CustomerID.ListSource = Customers_;
            cboOrders_CustomerID.ValueColumn = Customers_.CustomerID;
            cboOrders_CustomerID.DisplayColumn = Customers_.CompanyName;
            cboOrders_CustomerID.ListOrderBy = Customers_.SortByCompanyName;
        }
        void btn_Click(object sender, ButtonClickEventArgs e)
        {
            e.Raise(_controller.ExpandCust);
        }
        void btnPrint_Click(object sender, ButtonClickEventArgs e)
        {
            e.Raise(_controller.Print);
        }
    }
}
