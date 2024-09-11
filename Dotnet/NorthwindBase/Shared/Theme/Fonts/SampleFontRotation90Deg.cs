using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Sample Font rotation 90 Deg. #102</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Sample Font rotation 90 Deg.")]
    class SampleFontRotation90Deg : LoadableFontScheme 
    {
        public SampleFontRotation90Deg()
        {
            try
            {
                this.Font = new System.Drawing.Font("Arial", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
                this.TextAngle = 90;
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
