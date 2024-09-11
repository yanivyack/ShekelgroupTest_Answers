using ENV.Data.DataProvider;
namespace Northwind.Shared
{
    public class DataSources
    {
        public static DynamicSQLSupportingDataProvider Northwind 
        {
            get
            {
                return ConnectionManager.GetSQLDataProvider("Northwind");
            }
        }
    }
}
