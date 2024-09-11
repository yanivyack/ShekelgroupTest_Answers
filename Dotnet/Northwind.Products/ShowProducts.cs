using Firefly.Box;
using ENV.Data;
using ENV;
using Firefly.Box.Flow;
namespace Northwind.Products
{
    /// <summary>Show Products(P#4)</summary>
    /// <remark>Last change before Migration: 28/10/2013 22:01:15</remark>
    public class ShowProducts : UIControllerBase,IShowProducts 
    {
        #region Models
        internal readonly Models.Products Products = new Models.Products { AllowRowLocking = true };
        #endregion
        #region Parameters
        readonly Types.ProdID pi_ProdID = new Types.ProdID("pi.Prod ID");
        #endregion
        public ShowProducts()
        {
            Title = "Show Products";
            InitializeDataView();
        }
        void InitializeDataView()
        {
            From = Products;
            OrderBy = Products.SortByPK_Products;
            #region Column Selection
            // parameter for selection task
            Columns.Add(pi_ProdID);
            
            Columns.Add(Products.ProductID);
            Columns.Add(Products.ProductName);
            Columns.Add(Products.UnitPrice);
            MarkParameterColumns(pi_ProdID);
            #endregion
        }
        /// <summary>Show Products(P#4)</summary>
        public void Run(NumberParameter ppi_ProdID = null)
        {
            BindParameter(pi_ProdID, ppi_ProdID);
            Execute();
        }
        protected override void OnLoad()
        {
            RowLocking = LockingStrategy.OnRowSaving;
            TransactionScope = TransactionScopes.SaveToDatabase;
            Activity = Activities.Browse;
            SwitchToInsertWhenNoRows = true;
            AllowSelect = true;
            View = () => new Views.ShowProductsView(this);
        }
        protected override void OnSavingRow()
        {
            if (u.KBGet(1) == Command.Select)
                pi_ProdID.Value = Products.ProductID;
        }
    }
}
