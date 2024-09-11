using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Push Button Text #27</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Push Button Text")]
    class PushButtonText : LoadableFontScheme 
    {
        public PushButtonText()
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
