using System.Drawing;
using ENV.UI;
namespace Northwind.Shared.Theme.Fonts
{
    /// <summary>Table Title #24</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Table Title")]
    class TableTitle : LoadableFontScheme 
    {
        public TableTitle()
        {
            try
            {
                this.Font = new System.Drawing.Font("MS Sans Serif", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e);
            }
        }
    }
}
