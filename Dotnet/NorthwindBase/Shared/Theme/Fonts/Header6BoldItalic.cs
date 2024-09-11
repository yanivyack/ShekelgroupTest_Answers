using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Header 6 Bold Italic #77</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Header 6 Bold Italic")]
    public class Header6BoldItalic : LoadableFontScheme 
    {
        public Header6BoldItalic()
        {
            try
            {
                this.Font = new System.Drawing.Font("Times New Roman", 8F, FontStyle.Bold|FontStyle.Italic, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
