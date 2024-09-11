using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Header 6 Bold #63</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Header 6 Bold")]
    public class Header6Bold : LoadableFontScheme 
    {
        public Header6Bold()
        {
            try
            {
                this.Font = new System.Drawing.Font("Times New Roman", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
