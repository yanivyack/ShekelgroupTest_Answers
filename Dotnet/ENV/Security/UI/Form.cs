using System.Drawing;
using Firefly.Box.UI;
using Firefly.Box;
namespace ENV.Security.UI
{

    /// <summary>UserForm</summary>
    public partial class Form : Firefly.Box.UI.Form
    {
        /// <summary>UserForm</summary>
        public Form()
        {
            InitializeComponent();
            RightToLeft = LocalizationInfo.Current.RightToLeft;
            Modal = true;
        }
    }
}
