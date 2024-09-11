using Firefly.Box;
using ENV.Data;
namespace Northwind.Models
{
    /// <summary>Customers(E#1)</summary>
    public class Customers : Entity 
    {
        #region Columns
        [PrimaryKey]
        public readonly Types.CustomerID CustomerID = new Types.CustomerID("CustomerID");
        public readonly TextColumn CompanyName = new TextColumn("CompanyName", "40");
        public readonly TextColumn ContactName = new TextColumn("ContactName", "30");
        public readonly TextColumn ContactTitle = new TextColumn("ContactTitle", "30");
        public readonly TextColumn Address = new TextColumn("Address", "60");
        public readonly TextColumn City = new TextColumn("City", "15");
        public readonly TextColumn Region = new TextColumn("Region", "15");
        public readonly TextColumn PostalCode = new TextColumn("PostalCode", "10");
        public readonly TextColumn Country = new TextColumn("Country", "15");
        public readonly TextColumn Phone = new TextColumn("Phone", "24");
        public readonly TextColumn Fax = new TextColumn("Fax", "24");
        #endregion
        #region Indexes
        /// <summary>PK_Customers (#1)</summary>
        public readonly Index SortByPK_Customers = new Index { Caption = "PK_Customers", Name = "PK_Customers", AutoCreate = true, Unique = true };
        /// <summary>City (#2)</summary>
        public readonly Index SortByCity = new Index { Caption = "City", Name = "City", AutoCreate = true };
        /// <summary>CompanyName (#3)</summary>
        public readonly Index SortByCompanyName = new Index { Caption = "CompanyName", Name = "CompanyName", AutoCreate = true };
        /// <summary>PostalCode (#4)</summary>
        public readonly Index SortByPostalCode = new Index { Caption = "PostalCode", Name = "PostalCode", AutoCreate = true };
        /// <summary>Region (#5)</summary>
        public readonly Index SortByRegion = new Index { Caption = "Region", Name = "Region", AutoCreate = true };
        #endregion
        public Customers() : base("dbo.Customers", "Customers", Shared.DataSources.Northwind)
        {
            Cached = false;
            CustomerID.ClearExpandEvent();
            InitializeIndexes();
        }
        void InitializeIndexes()
        {
            SortByPK_Customers.Add(CustomerID);
            SortByCity.Add(City);
            SortByCompanyName.Add(CompanyName);
            SortByPostalCode.Add(PostalCode);
            SortByRegion.Add(Region);
        }
    }
}
