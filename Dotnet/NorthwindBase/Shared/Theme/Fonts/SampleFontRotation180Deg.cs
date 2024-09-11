using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Sample Font rotation 180 Deg. #104</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Sample Font rotation 180 Deg.")]
    class SampleFontRotation180Deg : LoadableFontScheme 
    {
        public SampleFontRotation180Deg()
        {
            try
            {
                this.Font = new System.Drawing.Font("Arial", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
                this.TextAngle = 180;
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
