using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Default Table Edit Field #1</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Default Table Edit Field")]
    public class DefaultTableEditField : LoadableFontScheme 
    {
        public DefaultTableEditField()
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
