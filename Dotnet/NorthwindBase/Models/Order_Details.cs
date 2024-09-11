using Firefly.Box;
using ENV.Data;
namespace Northwind.Models
{
    /// <summary>Order_Details(E#3)</summary>
    public class Order_Details : Entity 
    {
        #region Columns
        [PrimaryKey]
        public readonly Types.OrderID OrderID = new Types.OrderID("OrderID");
        [PrimaryKey]
        public readonly NumberColumn ProductID = new NumberColumn("ProductID", "N10");
        public readonly NumberColumn UnitPrice = new NumberColumn("UnitPrice", "10.3");
        public readonly NumberColumn Quantity = new NumberColumn("Quantity", "N5");
        public readonly NumberColumn Discount = new NumberColumn("Discount", "5.2");
        #endregion
        #region Indexes
        /// <summary>PK_Order_Details (#1)</summary>
        public readonly Index SortByPK_Order_Details = new Index { Caption = "PK_Order_Details", Name = "PK_Order_Details", AutoCreate = true, Unique = true };
        /// <summary>OrderID (#2)</summary>
        public readonly Index SortByOrderID = new Index { Caption = "OrderID", Name = "OrderID", AutoCreate = true };
        /// <summary>ProductID (#3)</summary>
        public readonly Index SortByProductID = new Index { Caption = "ProductID", Name = "ProductID", AutoCreate = true };
        #endregion
        public Order_Details() : base("dbo.[Order Details]", "Order_Details", Shared.DataSources.Northwind)
        {
            Cached = false;
            InitializeIndexes();
        }
        void InitializeIndexes()
        {
            SortByPK_Order_Details.Add(OrderID, ProductID);
            SortByOrderID.Add(OrderID);
            SortByProductID.Add(ProductID);
        }
    }
}
