using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Radio Button Default Font #10</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Radio Button Default Font")]
    public class RadioButtonDefaultFont : LoadableFontScheme 
    {
        public RadioButtonDefaultFont()
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
