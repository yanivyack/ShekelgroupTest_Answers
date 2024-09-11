using Firefly.Box;
using ENV.Data;
using ENV;
namespace Northwind.Orders
{
    /// <summary>Print - Order(P#6)</summary>
    /// <remark>Last change before Migration: 31/10/2009 09:09:27</remark>
    public class Print_Order : BusinessProcessBase 
    {
        #region Models
        internal readonly Models.Order_Details Order_Details = new Models.Order_Details { ReadOnly = true };
        internal readonly Models.Orders Orders = new Models.Orders { Cached = true, ReadOnly = true };
        internal readonly Models.Products Products = new Models.Products { Cached = true, ReadOnly = true };
        #endregion
        #region Parameters
        readonly Types.OrderID pi_OrderID = new Types.OrderID("pi.Order ID");
        #endregion
        #region Columns
        internal readonly Types.Amount vOrderTotal = new Types.Amount("vOrderTotal");
        #endregion
        #region Streams
        ENV.Printing.PrinterWriter _ioPrint_Order;
        #endregion
        #region Layouts
        Printing.Print_OrderC1 _layout => Cached<Printing.Print_OrderC1>();
        #endregion
        public Print_Order()
        {
            Title = "Print - Order";
            InitializeDataView();
            InitializeGroups();
        }
        void InitializeDataView()
        {
            From = Order_Details;
            
            Relations.Add(Orders, Orders.OrderID.IsEqualTo(Order_Details.OrderID), Orders.SortByPK_Orders);
            
            Relations.Add(Products, Products.ProductID.IsEqualTo(Order_Details.ProductID), Products.SortByPK_Products);
            
            Where.Add(CndRange(() => pi_OrderID != 0, Order_Details.OrderID.IsEqualTo(pi_OrderID)));
            
            OrderBy = Order_Details.SortByOrderID;
            #region Column Selection
            Columns.Add(pi_OrderID);
            // Range on Order if in parameters
            Columns.Add(Order_Details.OrderID);
            
            Columns.Add(Orders.OrderID);
            Columns.Add(Orders.CustomerID);
            Columns.Add(Orders.OrderDate);
            Columns.Add(Orders.RequiredDate);
            Columns.Add(Orders.ShippedDate);
            Columns.Add(Orders.ShipName);
            Columns.Add(Orders.ShipAddress);
            Columns.Add(Orders.ShipCity);
            Columns.Add(Orders.ShipCountry);
            
            Columns.Add(Order_Details.ProductID);
            
            Columns.Add(Products.ProductID);
            Columns.Add(Products.ProductName);
            
            Columns.Add(Order_Details.UnitPrice);
            Columns.Add(Order_Details.Quantity);
            Columns.Add(Order_Details.Discount);
            
            Columns.Add(vOrderTotal);
            MarkParameterColumns(pi_OrderID);
            #endregion
        }
        /// <summary>Print - Order(P#6)</summary>
        public void Run(NumberParameter ppi_OrderID)
        {
            BindParameter(pi_OrderID, ppi_OrderID);
            Execute();
        }
        protected override void OnLoad()
        {
            RowLocking = LockingStrategy.OnRowLoading;
            TransactionScope = TransactionScopes.Task;
            OnDatabaseErrorRetry = true;
            Activity = Activities.Browse;
            AllowUserAbort = true;
            
            _ioPrint_Order = new ENV.Printing.PrinterWriter() { Name = "Print - Order", PrinterName = Shared.Printing.Printers.Printer1.PrinterName, PrintPreview = true };
            Streams.Add(_ioPrint_Order);
        }
        void InitializeGroups()
        {
            Groups.Add(Order_Details.OrderID).Enter += () => 
            #region
            {
                vOrderTotal.Value = 0;
                _ioPrint_Order.NewPage();
                _layout.Header.WriteTo(_ioPrint_Order);
            };
            #endregion
            Groups[Order_Details.OrderID].Leave += () => 
            #region
            {
                _layout.Footer.WriteTo(_ioPrint_Order);
            };
            #endregion
        }
        protected override void OnLeaveRow()
        {
            if (Order_Details.Quantity > 0)
            {
                vOrderTotal.Value = vOrderTotal + Order_Details.UnitPrice * Order_Details.Quantity - Order_Details.Discount;
                _layout.Detail.WriteTo(_ioPrint_Order);
            }
        }
        #region Expressions
        internal Number Exp_1() => _ioPrint_Order.Page;
        
        internal Date Exp_2() => Date.Now;
        
        internal Number Exp_5() => Order_Details.UnitPrice * Order_Details.Quantity - Order_Details.Discount;
        
        #endregion
    }
}
