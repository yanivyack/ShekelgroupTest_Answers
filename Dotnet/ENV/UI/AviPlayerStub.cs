using System.Drawing;
using System.Windows.Forms;

namespace ENV.UI
{
    partial class AviPlayer
    {
        public static void Show(Control button, string aviFilePath)
        {
            PrivateShow(button, aviFilePath);
        }
        static partial void PrivateShow(Control button, string aviFilePath);
    }
}
