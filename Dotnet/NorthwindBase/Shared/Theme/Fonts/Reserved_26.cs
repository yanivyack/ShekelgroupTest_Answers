using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Reserved #85</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Reserved")]
    class Reserved_26 : LoadableFontScheme 
    {
        public Reserved_26()
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
