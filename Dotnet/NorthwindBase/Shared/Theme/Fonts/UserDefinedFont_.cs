using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>User Defined Font #110</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("User Defined Font")]
    class UserDefinedFont_ : LoadableFontScheme 
    {
        public UserDefinedFont_()
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
