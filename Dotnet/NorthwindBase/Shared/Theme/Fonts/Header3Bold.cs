using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Header 3 Bold #60</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Header 3 Bold")]
    public class Header3Bold : LoadableFontScheme 
    {
        public Header3Bold()
        {
            try
            {
                this.Font = new System.Drawing.Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
