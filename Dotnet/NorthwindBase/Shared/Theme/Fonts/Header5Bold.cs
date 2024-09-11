using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Header 5 Bold #62</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Header 5 Bold")]
    public class Header5Bold : LoadableFontScheme 
    {
        public Header5Bold()
        {
            try
            {
                this.Font = new System.Drawing.Font("Times New Roman", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
