using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>HTML Default Bold #57</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("HTML Default Bold")]
    public class HTML_DefaultBold : LoadableFontScheme 
    {
        public HTML_DefaultBold()
        {
            try
            {
                this.Font = new System.Drawing.Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
