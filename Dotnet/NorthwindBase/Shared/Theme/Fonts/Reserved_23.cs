using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Reserved #79</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Reserved")]
    class Reserved_23 : LoadableFontScheme 
    {
        public Reserved_23()
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
