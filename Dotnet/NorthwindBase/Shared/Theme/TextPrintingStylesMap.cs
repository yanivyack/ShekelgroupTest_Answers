using System.Drawing;
using Firefly.Box.UI;
using Firefly.Box;
using System.Collections.Generic;
namespace Northwind.Shared.Theme
{
    /// <summary>Map of TextPrintingStyles used to resolve value by index</summary>
    public class TextPrintingStylesMap
    {
        static TextPrintingStylesMap()
        {
        }
        /// <summary>Used to find TextPrintingStyles by index</summary>
        public static ENV.IO.Advanced.TextPrintingStyle Find(Number index)
        {
            if (index==null||!_map.ContainsKey(index))
                return null;
            return _map[index];
        }
        static Dictionary<Number,ENV.IO.Advanced.TextPrintingStyle> _map = new Dictionary<Number,ENV.IO.Advanced.TextPrintingStyle>();
    }
}
