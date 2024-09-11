using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Header 2 Bold Italic #73</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Header 2 Bold Italic")]
    public class Header2BoldItalic : LoadableFontScheme 
    {
        public Header2BoldItalic()
        {
            try
            {
                this.Font = new System.Drawing.Font("Times New Roman", 16F, FontStyle.Bold|FontStyle.Italic, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
