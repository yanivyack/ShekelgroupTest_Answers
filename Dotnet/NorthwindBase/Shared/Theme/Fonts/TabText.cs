using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Tab Text #21</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Tab Text")]
    class TabText : LoadableFontScheme 
    {
        public TabText()
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
