using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Header 3 Italic #67</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Header 3 Italic")]
    public class Header3Italic : LoadableFontScheme 
    {
        public Header3Italic()
        {
            try
            {
                this.Font = new System.Drawing.Font("Times New Roman", 14F, FontStyle.Italic, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
