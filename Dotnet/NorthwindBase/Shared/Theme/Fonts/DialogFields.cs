using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Dialog Fields #25</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Dialog Fields")]
    class DialogFields : LoadableFontScheme 
    {
        public DialogFields()
        {
            try
            {
                this.Font = new System.Drawing.Font("MS Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
