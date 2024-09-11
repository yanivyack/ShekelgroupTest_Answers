using Firefly.Box;
using ENV.Data;
namespace Northwind.Types
{
    /// <summary>Date(T#5)</summary>
    public class Date : DateColumn 
    {
        public Date(string name = "Date", string format = null, string caption = null) : base(name, format, caption)
        {
            DefaultValue = Firefly.Box.Date.Empty;
        }
    }
}
