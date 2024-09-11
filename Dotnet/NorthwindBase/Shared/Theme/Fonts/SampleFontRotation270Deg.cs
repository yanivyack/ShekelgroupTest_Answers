using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Sample Font rotation 270 Deg. #106</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Sample Font rotation 270 Deg.")]
    class SampleFontRotation270Deg : LoadableFontScheme 
    {
        public SampleFontRotation270Deg()
        {
            try
            {
                this.Font = new System.Drawing.Font("Arial", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
                this.TextAngle = 270;
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
