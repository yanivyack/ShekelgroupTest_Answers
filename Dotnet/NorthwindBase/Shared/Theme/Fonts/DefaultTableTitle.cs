using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Default Table Title #2</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Default Table Title")]
    public class DefaultTableTitle : LoadableFontScheme 
    {
        public DefaultTableTitle()
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
