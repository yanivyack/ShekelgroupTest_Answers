using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Fixed Size Font #33</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Fixed Size Font")]
    class FixedSizeFont : LoadableFontScheme 
    {
        public FixedSizeFont()
        {
            try
            {
                this.Font = new System.Drawing.Font("Courier New", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
