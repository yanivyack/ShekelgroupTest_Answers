using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Header 2 Italic #66</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Header 2 Italic")]
    public class Header2Italic : LoadableFontScheme 
    {
        public Header2Italic()
        {
            try
            {
                this.Font = new System.Drawing.Font("Times New Roman", 16F, FontStyle.Italic, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
