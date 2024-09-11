using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Default Dialog Text #4</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Default Dialog Text")]
    public class DefaultDialogText : LoadableFontScheme 
    {
        public DefaultDialogText()
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
