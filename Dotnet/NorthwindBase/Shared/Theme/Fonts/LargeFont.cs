using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Large Font #32</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Large Font")]
    class LargeFont : LoadableFontScheme 
    {
        public LargeFont()
        {
            try
            {
                this.Font = new System.Drawing.Font("MS Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
