using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Tooltips #22</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Tooltips")]
    class Tooltips : LoadableFontScheme 
    {
        public Tooltips()
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
