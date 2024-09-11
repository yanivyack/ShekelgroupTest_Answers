using Firefly.Box;
using ENV.Data;
namespace Northwind.Types
{
    /// <summary>Order ID(T#2)</summary>
    public class OrderID : NumberColumn 
    {
        public OrderID(string name = "Order ID", string format = "10", string caption = null) : base(name, format, caption)
        {
        }
    }
}
