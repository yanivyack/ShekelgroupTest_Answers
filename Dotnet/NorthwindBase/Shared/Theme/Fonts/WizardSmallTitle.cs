using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Wizard small title #82</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Wizard small title")]
    public class WizardSmallTitle : LoadableFontScheme 
    {
        public WizardSmallTitle()
        {
            try
            {
                this.Font = new System.Drawing.Font("Arial", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
