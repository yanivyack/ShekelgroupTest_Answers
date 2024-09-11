using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Header 5 Italic #69</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Header 5 Italic")]
    public class Header5Italic : LoadableFontScheme 
    {
        public Header5Italic()
        {
            try
            {
                this.Font = new System.Drawing.Font("Times New Roman", 10F, FontStyle.Italic, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
