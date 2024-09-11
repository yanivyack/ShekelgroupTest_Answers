using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Default Table Name Title #5</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Default Table Name Title")]
    class DefaultTableNameTitle : LoadableFontScheme 
    {
        public DefaultTableNameTitle()
        {
            try
            {
                this.Font = new System.Drawing.Font("MS Sans Serif", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
