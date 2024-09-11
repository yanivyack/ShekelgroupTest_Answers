using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>As Data Property #36</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("As Data Property")]
    class AsDataProperty : LoadableFontScheme 
    {
        public AsDataProperty()
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
