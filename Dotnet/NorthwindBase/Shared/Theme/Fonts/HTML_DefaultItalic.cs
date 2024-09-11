using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>HTML Default Italic #64</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("HTML Default Italic")]
    public class HTML_DefaultItalic : LoadableFontScheme 
    {
        public HTML_DefaultItalic()
        {
            try
            {
                this.Font = new System.Drawing.Font("Arial", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
