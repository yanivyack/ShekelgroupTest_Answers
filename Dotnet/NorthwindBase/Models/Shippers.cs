using Firefly.Box;
using ENV.Data;
namespace Northwind.Models
{
    /// <summary>Shippers(E#6)</summary>
    public class Shippers : Entity 
    {
        #region Columns
        [PrimaryKey]
        public readonly NumberColumn ShipperID = new NumberColumn("ShipperID", "N10") { NullDisplayText = "" };
        public readonly TextColumn CompanyName = new TextColumn("CompanyName", "40") { NullDisplayText = "" };
        public readonly TextColumn Phone = new TextColumn("Phone", "24") { NullDisplayText = "" };
        #endregion
        #region Indexes
        /// <summary>PK_Shippers (#1)</summary>
        public readonly Index SortByPK_Shippers = new Index { Caption = "PK_Shippers", Name = "PK_Shippers", AutoCreate = true, Unique = true };
        #endregion
        public Shippers() : base("dbo.Shippers", "Shippers", Shared.DataSources.Northwind)
        {
            Cached = false;
            InitializeIndexes();
        }
        void InitializeIndexes()
        {
            SortByPK_Shippers.Add(ShipperID);
        }
    }
}
