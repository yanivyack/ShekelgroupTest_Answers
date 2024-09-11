using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Sample Font rotation 135 Deg. #103</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Sample Font rotation 135 Deg.")]
    class SampleFontRotation135Deg : LoadableFontScheme 
    {
        public SampleFontRotation135Deg()
        {
            try
            {
                this.Font = new System.Drawing.Font("Arial", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
                this.TextAngle = 135;
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
