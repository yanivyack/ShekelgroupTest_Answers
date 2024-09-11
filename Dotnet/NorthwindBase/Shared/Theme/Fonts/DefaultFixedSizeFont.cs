using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Default Fixed Size Font #7</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Default Fixed Size Font")]
    public class DefaultFixedSizeFont : LoadableFontScheme 
    {
        public DefaultFixedSizeFont()
        {
            try
            {
                this.Font = new System.Drawing.Font("Courier New", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
