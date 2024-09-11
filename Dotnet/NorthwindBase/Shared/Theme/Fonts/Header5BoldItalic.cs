using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Header 5 Bold Italic #76</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Header 5 Bold Italic")]
    public class Header5BoldItalic : LoadableFontScheme 
    {
        public Header5BoldItalic()
        {
            try
            {
                this.Font = new System.Drawing.Font("Times New Roman", 10F, FontStyle.Bold|FontStyle.Italic, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
