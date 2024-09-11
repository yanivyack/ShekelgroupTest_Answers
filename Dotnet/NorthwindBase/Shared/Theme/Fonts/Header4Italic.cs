using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Header 4 Italic #68</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Header 4 Italic")]
    public class Header4Italic : LoadableFontScheme 
    {
        public Header4Italic()
        {
            try
            {
                this.Font = new System.Drawing.Font("Times New Roman", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
