using Firefly.Box;
using ENV.Data;
namespace Northwind.Types
{
    /// <summary>Prod ID(T#3)</summary>
    public class ProdID : NumberColumn 
    {
        public ProdID(string name = "Prod ID", string format = "10", string caption = null) : base(name, format, caption)
        {
            Expand += () => Create<Products.IShowProducts>().Run(this);
        }
    }
}
