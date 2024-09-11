using Firefly.Box;
using ENV.Data;
namespace Northwind.Types
{
    /// <summary>Customer ID(T#1)</summary>
    public class CustomerID : TextColumn 
    {
        public CustomerID(string name = "Customer ID", string format = "5", string caption = null) : base(name, format, caption)
        {
            Expand += () => Create<Customers.IShowCustomers>().Run(this);
        }
    }
}
