using Firefly.Box;
using ENV.Data;
using ENV;
using Firefly.Box.Flow;
namespace Northwind.Customers
{
    /// <summary>Show Customers(P#3)</summary>
    /// <remark>Last change before Migration: 28/10/2013 22:01:14</remark>
    public class ShowCustomers : UIControllerBase,IShowCustomers 
    {
        #region Models
        internal readonly Models.Customers Customers = new Models.Customers { AllowRowLocking = true };
        #endregion
        #region Parameters
        readonly Types.CustomerID pi_CustomerID = new Types.CustomerID("pi.Customer ID");
        #endregion
        public ShowCustomers()
        {
            Title = "Show Customers";
            InitializeDataView();
        }
        void InitializeDataView()
        {
            From = Customers;
            OrderBy = Customers.SortByPK_Customers;
            #region Column Selection
            // parameter for selection task
            Columns.Add(pi_CustomerID);
            
            Columns.Add(Customers.CustomerID);
            Columns.Add(Customers.CompanyName);
            Columns.Add(Customers.Address);
            Columns.Add(Customers.City);
            Columns.Add(Customers.Phone);
            MarkParameterColumns(pi_CustomerID);
            #endregion
        }
        /// <summary>Show Customers(P#3)</summary>
        public void Run(TextParameter ppi_CustomerID = null)
        {
            BindParameter(pi_CustomerID, ppi_CustomerID);
            Execute();
        }
        protected override void OnLoad()
        {
            RowLocking = LockingStrategy.OnRowSaving;
            TransactionScope = TransactionScopes.SaveToDatabase;
            BindAllowInsert(() => Customers.City != "Madrid");
            SwitchToInsertWhenNoRows = true;
            AllowExportData = true;
            AllowSelect = true;
            View = () => new Views.ShowCustomersView(this);
        }
        protected override void OnSavingRow()
        {
            if (u.KBGet(1) == Command.Select)
                pi_CustomerID.Value = Customers.CustomerID;
        }
    }
}
