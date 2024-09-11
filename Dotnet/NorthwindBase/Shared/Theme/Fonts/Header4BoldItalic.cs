using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Header 4 Bold Italic #75</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Header 4 Bold Italic")]
    public class Header4BoldItalic : LoadableFontScheme 
    {
        public Header4BoldItalic()
        {
            try
            {
                this.Font = new System.Drawing.Font("Times New Roman", 12F, FontStyle.Bold|FontStyle.Italic, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
