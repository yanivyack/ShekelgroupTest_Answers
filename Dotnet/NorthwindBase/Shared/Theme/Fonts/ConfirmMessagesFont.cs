using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Confirm Messages Font #29</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Confirm Messages Font")]
    class ConfirmMessagesFont : LoadableFontScheme 
    {
        public ConfirmMessagesFont()
        {
            try
            {
                this.Font = new System.Drawing.Font("MS Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
