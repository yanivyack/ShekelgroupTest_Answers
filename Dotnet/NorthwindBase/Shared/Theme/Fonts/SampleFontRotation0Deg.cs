using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Sample Font rotation 0 Deg. #108</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Sample Font rotation 0 Deg.")]
    class SampleFontRotation0Deg : LoadableFontScheme 
    {
        public SampleFontRotation0Deg()
        {
            try
            {
                this.Font = new System.Drawing.Font("Arial", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
