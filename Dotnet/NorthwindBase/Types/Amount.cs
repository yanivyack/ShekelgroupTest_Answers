using Firefly.Box;
using ENV.Data;
namespace Northwind.Types
{
    /// <summary>Amount(T#4)</summary>
    public class Amount : NumberColumn 
    {
        public Amount(string name = "Amount", string format = "5.2CZ +$;", string caption = null) : base(name, format, caption)
        {
        }
    }
}
