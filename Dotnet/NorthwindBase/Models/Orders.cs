using Firefly.Box;
using ENV.Data;
namespace Northwind.Models
{
    /// <summary>Orders(E#2)</summary>
    public class Orders : Entity 
    {
        #region Columns
        [PrimaryKey]
        public readonly Types.OrderID OrderID = new Types.OrderID("OrderID") { NullDisplayText = "" };
        public readonly Types.CustomerID CustomerID = new Types.CustomerID("CustomerID") { NullDisplayText = "" };
        public readonly NumberColumn EmployeeID = new NumberColumn("EmployeeID", "N10") { AllowNull = false, NullDisplayText = "", DbType = "INTEGER" };
        public readonly Types.Date OrderDate = new Types.Date("OrderDate") { Format = "##/##/####", NullDisplayText = "", DefaultValue = new Date(1901,1,1), Storage = new ENV.Data.Storage.DateTimeDateStorage() };
        public readonly Types.Date RequiredDate = new Types.Date("RequiredDate") { Format = "##/##/####", NullDisplayText = "", DefaultValue = new Date(1901,1,1), Storage = new ENV.Data.Storage.DateTimeDateStorage() };
        public readonly Types.Date ShippedDate = new Types.Date("ShippedDate") { Format = "##/##/####", NullDisplayText = "", DefaultValue = new Date(1901,1,1), Storage = new ENV.Data.Storage.DateTimeDateStorage() };
        public readonly NumberColumn ShipVia = new NumberColumn("ShipVia", "N10") { NullDisplayText = "", DbType = "INTEGER" };
        public readonly NumberColumn Freight = new NumberColumn("Freight", "10.3") { NullDisplayText = "", DbType = "MONEY", DbDefault = "0" };
        public readonly TextColumn ShipName = new TextColumn("ShipName", "40") { NullDisplayText = "" };
        public readonly TextColumn ShipAddress = new TextColumn("ShipAddress", "60") { NullDisplayText = "" };
        public readonly TextColumn ShipCity = new TextColumn("ShipCity", "15") { NullDisplayText = "" };
        public readonly TextColumn ShipRegion = new TextColumn("ShipRegion", "15") { NullDisplayText = "" };
        public readonly TextColumn ShipPostalCode = new TextColumn("ShipPostalCode", "10") { NullDisplayText = "" };
        public readonly TextColumn ShipCountry = new TextColumn("ShipCountry", "15") { NullDisplayText = "" };
        #endregion
        #region Indexes
        /// <summary>PK_Orders (#1)</summary>
        public readonly Index SortByPK_Orders = new Index { Caption = "PK_Orders", Name = "PK_Orders", AutoCreate = true, Unique = true };
        /// <summary>CustomerID (#2)</summary>
        public readonly Index SortByCustomerID = new Index { Caption = "CustomerID", Name = "CustomerID", AutoCreate = true };
        #endregion
        public Orders() : base("dbo.Orders", "Orders", Shared.DataSources.Northwind)
        {
            Cached = false;
            InitializeIndexes();
        }
        void InitializeIndexes()
        {
            SortByPK_Orders.Add(OrderID);
            SortByCustomerID.Add(CustomerID, OrderDate);
        }
    }
}
