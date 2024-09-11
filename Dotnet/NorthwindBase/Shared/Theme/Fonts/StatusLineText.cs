using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Status Line Text #28</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Status Line Text")]
    class StatusLineText : LoadableFontScheme 
    {
        public StatusLineText()
        {
            try
            {
                this.Font = new System.Drawing.Font("MS Sans Serif", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
