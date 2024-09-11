using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Header 4 Bold #61</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Header 4 Bold")]
    public class Header4Bold : LoadableFontScheme 
    {
        public Header4Bold()
        {
            try
            {
                this.Font = new System.Drawing.Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
