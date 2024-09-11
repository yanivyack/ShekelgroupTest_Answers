using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Header 1 Italic #65</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Header 1 Italic")]
    public class Header1Italic : LoadableFontScheme 
    {
        public Header1Italic()
        {
            try
            {
                this.Font = new System.Drawing.Font("Times New Roman", 18F, FontStyle.Italic, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
