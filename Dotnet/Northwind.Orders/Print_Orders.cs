using Firefly.Box;
using ENV.Data;
using ENV;
namespace Northwind.Orders
{
    /// <summary>Print - Orders(P#7)</summary>
    /// <remark>Last change before Migration: 10/06/2008 16:03:35</remark>
    public class Print_Orders : BusinessProcessBase 
    {
        #region Models
        internal readonly Models.Orders Orders = new Models.Orders { ReadOnly = true };
        internal readonly Models.Customers Customers = new Models.Customers { Cached = true, AllowRowLocking = true };
        #endregion
        #region Streams
        ENV.Printing.PrinterWriter _ioPrint_Order;
        #endregion
        #region Layouts
        Printing.Print_OrdersC1 _layout => Cached<Printing.Print_OrdersC1>();
        #endregion
        public Print_Orders()
        {
            Title = "Print - Orders";
            InitializeDataView();
            InitializeGroups();
        }
        void InitializeDataView()
        {
            From = Orders;
            
            Relations.Add(Customers, Customers.CustomerID.IsEqualTo(Orders.CustomerID));
            
            OrderBy = Orders.SortByCustomerID;
            #region Column Selection
            Columns.Add(Orders.OrderID);
            Columns.Add(Orders.CustomerID);
            Columns.Add(Customers.CustomerID);
            Columns.Add(Customers.ContactName);
            Columns.Add(Orders.OrderDate);
            Columns.Add(Orders.RequiredDate);
            Columns.Add(Orders.ShippedDate);
            Columns.Add(Orders.ShipName);
            Columns.Add(Orders.ShipAddress);
            Columns.Add(Orders.ShipCity);
            Columns.Add(Orders.ShipCountry);
            #endregion
        }
        protected override void OnLoad()
        {
            RowLocking = LockingStrategy.OnRowLoading;
            TransactionScope = TransactionScopes.Task;
            OnDatabaseErrorRetry = true;
            Activity = Activities.Browse;
            AllowUserAbort = true;
            
            _ioPrint_Order = new ENV.Printing.PrinterWriter() { Name = "Print - Order", PageHeader = _layout.Header, PrinterName = Shared.Printing.Printers.Printer1.PrinterName, PrintPreview = true };
            Streams.Add(_ioPrint_Order);
        }
        void InitializeGroups()
        {
            Groups.Add(Orders.CustomerID).Enter += () => 
            #region
            {
                _layout.Customer.WriteTo(_ioPrint_Order);
            };
            #endregion
        }
        protected override void OnLeaveRow()
        {
            _layout.Body.WriteTo(_ioPrint_Order);
        }
        #region Expressions
        internal Number Exp_2() => _ioPrint_Order.Page;
        
        internal Date Exp_3() => Date.Now;
        
        #endregion
    }
}
