using Firefly.Box;
using ENV.Data;
using ENV;
using Firefly.Box.Flow;
using System.Windows.Forms;
using Message = ENV.Message;
using Firefly.Box.Advanced;
namespace Northwind.Orders
{
    /// <summary>Show Orders(P#5)</summary>
    /// <remark>Last change before Migration: 28/10/2013 22:01:17</remark>
    public class ShowOrders : FlowUIControllerBase,IShowOrders 
    {
        #region Models
        internal readonly Models.Orders Orders = new Models.Orders { AllowRowLocking = true };
        readonly Models.Customers Customers = new Models.Customers { Cached = true, ReadOnly = true };
        readonly Models.Orders Orders_ = new Models.Orders { Cached = true, AllowRowLocking = true };
        #endregion
        #region Columns
        internal readonly Types.Amount v_Total = new Types.Amount("v.Total");
        readonly Types.Amount v_TotalInTheLastWarning = new Types.Amount("v.Total in the last warning");
        #endregion
        #region CustomCommands
        internal readonly CustomCommand ExpandCust = new CustomCommand("Expand Cust");
        internal readonly CustomCommand Print = new CustomCommand("Print") { Precondition = CustomCommandPrecondition.LeaveRowAndSaveToDatabaseAfterHandlerInvokation };
        #endregion
        public ShowOrders()
        {
            Title = "Show Orders";
            InitializeDataViewAndUserFlow();
            InitializeHandlers();
        }
        void InitializeDataViewAndUserFlow()
        {
            From = Orders;
            
            Relations.Add(Customers, Customers.CustomerID.IsEqualTo(Orders.CustomerID), Customers.SortByPK_Customers);
            
            Relations.Add(Orders_, Orders_.SortByPK_Orders);
            Relations[Orders_].OrderBy.Reversed = true;
            
            OrderBy = Orders.SortByPK_Orders;
            #region Column Selection and User Flow
            Flow.Add<Customers.IShowCustomers>(c => c.Run(Orders.CustomerID), FlowMode.ExpandBefore);
            Columns.Add(Orders.CustomerID);
            
            Columns.Add(Customers.CustomerID);
            Columns.Add(Customers.CompanyName);
            Columns.Add(Customers.Address);
            Columns.Add(Customers.City);
            Columns.Add(Customers.Country);
            
            Columns.Add(Orders.OrderDate).BindValue(() => Date.Now);
            Flow.Add(() => Message.ShowError("Please enter a valid Required Date"), () => Orders.OrderDate <= new Date(1990, 1, 1));
            Columns.Add(Orders.RequiredDate).BindValue(() => Date.Now + 2);
            Flow.Add(() => Message.ShowError("Please enter a valid Required Date"), () => Orders.RequiredDate <= new Date(1990, 1, 1));
            Columns.Add(Orders.ShippedDate);
            Columns.Add(Orders.ShipName).BindValue(Customers.CompanyName);
            Columns.Add(Orders.ShipAddress).BindValue(Customers.Address);
            Columns.Add(Orders.ShipCity).BindValue(Customers.City);
            Columns.Add(Orders.ShipCountry).BindValue(Customers.Country);
            
            Columns.Add(v_Total);
            
            Columns.Add(v_TotalInTheLastWarning);
            
            // Get Last Order Number
            Columns.Add(Orders_.OrderID).Caption = "lnkLastOrderID";
            
            Columns.Add(Orders.OrderID).BindValue(() => Orders_.OrderID + 1);
            
            Flow.Add<Details>(c => c.Run(), FlowMode.Tab, Direction.Forward);
            #endregion
        }
        /// <summary>Show Orders(P#5)</summary>
        public void Run()
        {
            Execute();
        }
        protected override void OnLoad()
        {
            RowLocking = LockingStrategy.OnRowSaving;
            TransactionScope = TransactionScopes.SaveToDatabase;
            View = () => new Views.ShowOrdersView(this);
        }
        protected override void OnEnterRow()
        {
            Cached<Totals>().Run();
            v_TotalInTheLastWarning.SilentSet(v_Total);
            Cached<Details>().Run();
        }
        void InitializeHandlers()
        {
            Handlers.Add(Command.Expression(() => Exp_9())).Invokes += e => 
            #region
            {
                Message.ShowWarning("Warning: Order Exceed 1000$. Talk to your supervisor");
                v_TotalInTheLastWarning.SilentSet(v_Total);
            };
            #endregion
            Handlers.Add(Print).Invokes += e => 
            #region
            {
                // send range of this order
                new Print_Order().Run(Orders.OrderID);
                e.Handled = true;
            };
            #endregion
            Handlers.Add(ExpandCust).Invokes += e => 
            #region
            {
                // can expnad the customer details
                Create<Customers.IShowCustomers>().Run(Orders.CustomerID);
                e.Handled = true;
            };
            #endregion
        }
        #region Expressions
        Bool Exp_9() => v_Total > 1000 && v_TotalInTheLastWarning != v_Total;
        
        #endregion
        /// <summary>Details(P#5.1)</summary>
        /// <remark>Last change before Migration: 02/01/2008 09:11:36</remark>
        internal class Details : UIControllerBase 
        {
            #region Models
            internal readonly Models.Order_Details Order_Details = new Models.Order_Details { AllowRowLocking = true };
            readonly Models.Products Products = new Models.Products { Cached = true, ReadOnly = true };
            #endregion
            #region Columns
            readonly NumberColumn pi_HandlerParmPerecent = new NumberColumn("pi.HandlerParmPerecent", "1");
            #endregion
            #region CustomCommands
            internal readonly CustomCommand CalcDiscount = new CustomCommand("Calc Discount");
            #endregion
            ShowOrders _parent;
            public Details(ShowOrders parent)
            {
                _parent = parent;
                Title = "Details";
                InitializeDataView();
                InitializeHandlers();
            }
            void InitializeDataView()
            {
                From = Order_Details;
                
                Relations.Add(Products, Products.ProductID.IsEqualTo(Order_Details.ProductID), Products.SortByPK_Products);
                
                Where.Add(Order_Details.OrderID.BindEqualTo(_parent.Orders.OrderID));
                
                OrderBy = Order_Details.SortByPK_Order_Details;
                #region Column Selection
                Columns.Add(Order_Details.OrderID);
                Columns.Add(Order_Details.ProductID);
                
                Columns.Add(Products.ProductID);
                Columns.Add(Products.UnitPrice);
                
                Columns.Add(Order_Details.UnitPrice).BindValue(Products.UnitPrice);
                Columns.Add(Order_Details.Quantity);
                Columns.Add(Order_Details.Discount);
                #endregion
            }
            /// <summary>Details(P#5.1)</summary>
            public void Run()
            {
                Execute();
            }
            protected override void OnLoad()
            {
                Exit(ExitTiming.BeforeRow, () => u.Level(1) != "RM");
                RowLocking = LockingStrategy.OnUserEdit;
                TransactionScope = TransactionScopes.Row;
                SwitchToInsertWhenNoRows = true;
                KeepViewVisibleAfterExit = true;
                View = () => new Views.ShowOrdersDetails(this);
            }
            protected override void OnSavingRow()
            {
                _parent.v_Total.AddDeltaOf(() => Exp_3());
                u.DenyUndoFor(_parent.v_Total);
            }
            void InitializeHandlers()
            {
                Handlers.Add((Keys.Control|Keys.F10), HandlerScope.CurrentTaskOnly).Invokes += e => 
                #region
                {
                    Invoke(CalcDiscount, 10);
                    e.Handled = true;
                };
                #endregion
                Handlers.Add((Keys.Control|Keys.F5), HandlerScope.CurrentTaskOnly).Invokes += e => 
                #region
                {
                    Invoke(CalcDiscount, 5);
                    e.Handled = true;
                };
                #endregion
                var h = Handlers.Add(CalcDiscount, HandlerScope.CurrentTaskOnly);
                #region 
                h.Parameters.Add(pi_HandlerParmPerecent);
                h.Invokes += e => 
                {
                    LockCurrentRowIfItWasChanged();
                    Order_Details.Discount.SilentSet(Order_Details.UnitPrice * Order_Details.Quantity * pi_HandlerParmPerecent / 100);
                    e.Handled = true;
                };
                #endregion
            }
            #region Expressions
            internal Number Exp_3() => Order_Details.UnitPrice * Order_Details.Quantity - Order_Details.Discount;
            
            #endregion
        }
        /// <summary>Totals(P#5.2)</summary>
        /// <remark>Last change before Migration: 11/04/2007 20:09:10</remark>
        class Totals : BusinessProcessBase 
        {
            #region Models
            readonly Models.Order_Details Order_Details = new Models.Order_Details { AllowRowLocking = true };
            #endregion
            ShowOrders _parent;
            public Totals(ShowOrders parent)
            {
                _parent = parent;
                Title = "Totals";
                InitializeDataView();
            }
            void InitializeDataView()
            {
                From = Order_Details;
                Where.Add(Order_Details.OrderID.IsEqualTo(_parent.Orders.OrderID));
                
                OrderBy = Order_Details.SortByPK_Order_Details;
                #region Column Selection
                Columns.Add(Order_Details.OrderID);
                Columns.Add(Order_Details.UnitPrice);
                Columns.Add(Order_Details.Quantity);
                Columns.Add(Order_Details.Discount);
                #endregion
            }
            /// <summary>Totals(P#5.2)</summary>
            public void Run()
            {
                Execute();
            }
            protected override void OnLoad()
            {
                RowLocking = LockingStrategy.OnRowLoading;
                TransactionScope = TransactionScopes.Task;
            }
            protected override void OnStart()
            {
                _parent.v_Total.Value = 0;
            }
            protected override void OnLeaveRow()
            {
                _parent.v_Total.Value = _parent.v_Total + Order_Details.UnitPrice * Order_Details.Quantity - Order_Details.Discount;
            }
        }
    }
}
