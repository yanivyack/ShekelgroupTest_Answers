using System.Drawing;
namespace ENV.Labs.UI
{

    /// <summary>ScreenPicture</summary>
    public partial class ScreenPicture : ENV.UI.PictureBox
    {


        /// <summary>ScreenPicture</summary>
        public ScreenPicture()
        {
            InitializeComponent();
        }

        protected override Image GetImage(string imageLocation)
        {
            return ENV.Properties.Resources.Screen;
        }
    }
}
