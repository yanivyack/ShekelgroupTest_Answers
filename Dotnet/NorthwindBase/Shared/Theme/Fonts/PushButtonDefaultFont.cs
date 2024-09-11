using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Push Button Default Font #9</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Push Button Default Font")]
    public class PushButtonDefaultFont : LoadableFontScheme 
    {
        public PushButtonDefaultFont()
        {
            try
            {
                this.Font = new System.Drawing.Font("MS Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
