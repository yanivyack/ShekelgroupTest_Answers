using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>HTML Default Bold Italic #71</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("HTML Default Bold Italic")]
    public class HTML_DefaultBoldItalic : LoadableFontScheme 
    {
        public HTML_DefaultBoldItalic()
        {
            try
            {
                this.Font = new System.Drawing.Font("Arial", 12F, FontStyle.Bold|FontStyle.Italic, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
