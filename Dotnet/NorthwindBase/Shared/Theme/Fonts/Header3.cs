using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Header 3 #53</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Header 3")]
    public class Header3 : LoadableFontScheme 
    {
        public Header3()
        {
            try
            {
                this.Font = new System.Drawing.Font("Times New Roman", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
