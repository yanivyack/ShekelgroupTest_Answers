using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Header 2 Bold #59</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Header 2 Bold")]
    public class Header2Bold : LoadableFontScheme 
    {
        public Header2Bold()
        {
            try
            {
                this.Font = new System.Drawing.Font("Times New Roman", 16F, FontStyle.Bold, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
