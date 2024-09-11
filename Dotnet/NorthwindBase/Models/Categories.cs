using Firefly.Box;
using ENV.Data;
namespace Northwind.Models
{
    /// <summary>Categories(E#5)</summary>
    public class Categories : Entity 
    {
        #region Columns
        [PrimaryKey]
        public readonly NumberColumn CategoryID = new NumberColumn("CategoryID", "N10") { NullDisplayText = "" };
        public readonly TextColumn CategoryName = new TextColumn("CategoryName", "15") { NullDisplayText = "" };
        public readonly TextColumn Description = new TextColumn("Description", "10") { NullDisplayText = "" };
        #endregion
        #region Indexes
        /// <summary>PK_Categories (#1)</summary>
        public readonly Index SortByPK_Categories = new Index { Caption = "PK_Categories", Name = "PK_Categories", AutoCreate = true, Unique = true };
        /// <summary>CategoryName (#2)</summary>
        public readonly Index SortByCategoryName = new Index { Caption = "CategoryName", Name = "CategoryName", AutoCreate = true };
        #endregion
        public Categories() : base("dbo.Categories", "Categories", Shared.DataSources.Northwind)
        {
            Cached = false;
            InitializeIndexes();
        }
        void InitializeIndexes()
        {
            SortByPK_Categories.Add(CategoryID);
            SortByCategoryName.Add(CategoryName);
        }
    }
}
