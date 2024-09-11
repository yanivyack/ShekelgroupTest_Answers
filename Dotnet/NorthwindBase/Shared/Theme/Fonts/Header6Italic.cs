using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Header 6 Italic #70</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Header 6 Italic")]
    public class Header6Italic : LoadableFontScheme 
    {
        public Header6Italic()
        {
            try
            {
                this.Font = new System.Drawing.Font("Times New Roman", 8F, FontStyle.Italic, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
