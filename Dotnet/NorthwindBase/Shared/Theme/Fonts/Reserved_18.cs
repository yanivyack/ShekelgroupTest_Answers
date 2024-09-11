using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Reserved #46</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Reserved")]
    class Reserved_18 : LoadableFontScheme 
    {
        public Reserved_18()
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
