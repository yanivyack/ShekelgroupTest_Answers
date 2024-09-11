using System.Drawing;
using Firefly.Box.UI;
using Firefly.Box;
namespace ENV.Security.UI
{

    /// <summary>Grid</summary>
    public partial class Grid : ENV.UI.Grid
    {


        /// <summary>Grid</summary>
        public Grid()
        {
            InitializeComponent();
            EnableGridEnhancements();
            UnderConstructionNewGridLook = true;
            HorizontalScrollbar = false;
        }

    }
}
