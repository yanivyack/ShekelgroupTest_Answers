using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Header 1 Bold #58</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Header 1 Bold")]
    public class Header1Bold : LoadableFontScheme 
    {
        public Header1Bold()
        {
            try
            {
                this.Font = new System.Drawing.Font("Times New Roman", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
