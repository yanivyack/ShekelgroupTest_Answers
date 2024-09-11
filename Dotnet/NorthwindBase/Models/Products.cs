using Firefly.Box;
using ENV.Data;
namespace Northwind.Models
{
    /// <summary>Products(E#4)</summary>
    public class Products : Entity 
    {
        #region Columns
        [PrimaryKey]
        public readonly Types.ProdID ProductID = new Types.ProdID("ProductID") { Format = "N10" };
        public readonly TextColumn ProductName = new TextColumn("ProductName", "40");
        public readonly NumberColumn SupplierID = new NumberColumn("SupplierID", "N10");
        public readonly NumberColumn CategoryID = new NumberColumn("CategoryID", "N10");
        public readonly TextColumn QuantityPerUnit = new TextColumn("QuantityPerUnit", "20");
        public readonly NumberColumn UnitPrice = new NumberColumn("UnitPrice", "10.3");
        public readonly NumberColumn UnitsInStock = new NumberColumn("UnitsInStock", "N5");
        public readonly NumberColumn UnitsOnOrder = new NumberColumn("UnitsOnOrder", "N5");
        public readonly NumberColumn ReorderLevel = new NumberColumn("ReorderLevel", "N5");
        public readonly BoolColumn Discontinued = new BoolColumn("Discontinued", "5");
        #endregion
        #region Indexes
        /// <summary>PK_Products (#1)</summary>
        public readonly Index SortByPK_Products = new Index { Caption = "PK_Products", Name = "PK_Products", AutoCreate = true, Unique = true };
        /// <summary>CategoriesProducts (#2)</summary>
        public readonly Index SortByCategoriesProducts = new Index { Caption = "CategoriesProducts", Name = "CategoriesProducts", AutoCreate = true };
        /// <summary>ProductName (#3)</summary>
        public readonly Index SortByProductName = new Index { Caption = "ProductName", Name = "ProductName", AutoCreate = true };
        /// <summary>SupplierID (#4)</summary>
        public readonly Index SortBySupplierID = new Index { Caption = "SupplierID", Name = "SupplierID", AutoCreate = true };
        #endregion
        public Products() : base("dbo.Products", "Products", Shared.DataSources.Northwind)
        {
            Cached = false;
            ProductID.ClearExpandEvent();
            InitializeIndexes();
        }
        void InitializeIndexes()
        {
            SortByPK_Products.Add(ProductID);
            SortByCategoriesProducts.Add(CategoryID);
            SortByProductName.Add(ProductName);
            SortBySupplierID.Add(SupplierID);
        }
    }
}
