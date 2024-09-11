using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Wizard large title #81</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Wizard large title")]
    public class WizardLargeTitle : LoadableFontScheme 
    {
        public WizardLargeTitle()
        {
            try
            {
                this.Font = new System.Drawing.Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
