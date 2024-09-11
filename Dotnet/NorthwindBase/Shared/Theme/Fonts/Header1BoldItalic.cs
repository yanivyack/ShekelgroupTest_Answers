using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Header 1 Bold Italic #72</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Header 1 Bold Italic")]
    public class Header1BoldItalic : LoadableFontScheme 
    {
        public Header1BoldItalic()
        {
            try
            {
                this.Font = new System.Drawing.Font("Times New Roman", 18F, FontStyle.Bold|FontStyle.Italic, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
