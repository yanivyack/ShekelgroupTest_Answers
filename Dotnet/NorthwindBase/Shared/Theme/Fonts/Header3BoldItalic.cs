using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Header 3 Bold Italic #74</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Header 3 Bold Italic")]
    public class Header3BoldItalic : LoadableFontScheme 
    {
        public Header3BoldItalic()
        {
            try
            {
                this.Font = new System.Drawing.Font("Times New Roman", 14F, FontStyle.Bold|FontStyle.Italic, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
