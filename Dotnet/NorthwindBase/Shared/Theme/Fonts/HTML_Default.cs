using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>HTML Default #50</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("HTML Default")]
    public class HTML_Default : LoadableFontScheme 
    {
        public HTML_Default()
        {
            try
            {
                this.Font = new System.Drawing.Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
