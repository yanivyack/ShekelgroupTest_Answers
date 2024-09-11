using System.Drawing;
using Firefly.Box.UI;

namespace ENV.Labs.UI
{
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Default Dialog Edit Fields")]
    public class DefaultFont : FontScheme
    {
        public DefaultFont()
        {
            try
            {
                this.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            }
            catch
            {
            }
        }


    }
}
