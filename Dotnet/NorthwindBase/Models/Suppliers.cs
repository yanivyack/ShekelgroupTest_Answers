using Firefly.Box;
using ENV.Data;
namespace Northwind.Models
{
    /// <summary>Suppliers(E#7)</summary>
    public class Suppliers : Entity 
    {
        #region Columns
        [PrimaryKey]
        public readonly NumberColumn SupplierID = new NumberColumn("SupplierID", "N10") { NullDisplayText = "" };
        public readonly TextColumn CompanyName = new TextColumn("CompanyName", "40") { NullDisplayText = "" };
        #endregion
        #region Indexes
        /// <summary>PK_Suppliers (#1)</summary>
        public readonly Index SortByPK_Suppliers = new Index { Caption = "PK_Suppliers", Name = "PK_Suppliers", AutoCreate = true, Unique = true };
        /// <summary>CompanyName (#2)</summary>
        public readonly Index SortByCompanyName = new Index { Caption = "CompanyName", Name = "CompanyName", AutoCreate = true };
        #endregion
        public Suppliers() : base("dbo.Suppliers", "Suppliers", Shared.DataSources.Northwind)
        {
            Cached = false;
            InitializeIndexes();
        }
        void InitializeIndexes()
        {
            SortByPK_Suppliers.Add(SupplierID);
            SortByCompanyName.Add(CompanyName);
        }
    }
}
