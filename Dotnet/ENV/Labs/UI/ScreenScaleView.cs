#region Copyright Firefly Ltd 2014
/* *********************** DISCLAIMER **************************************
 * This software and documentation constitute an unpublished work and contain 
 * valuable trade secrets and proprietary information belonging to Firefly Ltd. 
 * None of the foregoing material may be copied, duplicated or disclosed without 
 * the express written permission of Firefly Ltd. 
 * FIREFLY LTD EXPRESSLY DISCLAIMS ANY AND ALL WARRANTIES CONCERNING THIS SOFTWARE 
 * AND DOCUMENTATION, INCLUDING ANY WARRANTIES OF MERCHANTABILITY AND/OR FITNESS 
 * FOR ANY PARTICULAR PURPOSE, AND WARRANTIES OF PERFORMANCE, AND ANY WARRANTY 
 * THAT MIGHT OTHERWISE ARISE FROM COURSE OF DEALING OR USAGE OF TRADE. NO WARRANTY 
 * IS EITHER EXPRESS OR IMPLIED WITH RESPECT TO THE USE OF THE SOFTWARE OR 
 * DOCUMENTATION. 
 * Under no circumstances shall Firefly Ltd be liable for incidental, special, 
 * indirect, direct or consequential damages or loss of profits, interruption of 
 * business, or related expenses which may arise from use of software or documentation, 
 * including but not limited to those resulting from defects in software and/or 
 * documentation, or loss or inaccuracy of data of any kind. 
 */
#endregion

using System.Drawing;
using Firefly.Box.UI;

namespace ENV.Labs.UI
{
    public partial class ScreenScaleView : ENV.UI.Form
    {
        
        private ScreenScale _controller;
        public ScreenScaleView(ScreenScale controller)
        {
            _controller = controller;
            InitializeComponent();

            lstSource.Values = "640 x 480, 800 x 600, 1024 x 768";
                //_controller.GetResolutions();
            lstTarget.Values = "1024 x 768, 1280 x 720, 1280 x 1024, 1920 x 1080";
            //_controller.GetResolutions();
        }

        private void btnOK_Click(object sender, Firefly.Box.UI.Advanced.ButtonClickEventArgs e)
        {
            _controller.Scale();
            
        }

        private void btnCancel_Click(object sender, Firefly.Box.UI.Advanced.ButtonClickEventArgs e)
        {
            Close();
        }

        private void pictureBox1_BindHeight(object sender, Firefly.Box.UI.Advanced.IntBindingEventArgs e)
        {
            e.Value = (int) (_controller.GetOriginalHeight()*ScalingFactor.Height);
        }

        private void pictureBox1_BindWidth(object sender, Firefly.Box.UI.Advanced.IntBindingEventArgs e)
        {
            e.Value = (int) (_controller.GetOriginalWidth()*ScalingFactor.Width);
        }

        private void pictureBox2_BindWidth(object sender, Firefly.Box.UI.Advanced.IntBindingEventArgs e)
        {
            e.Value = (int) (_controller.GetNewWidth()*ScalingFactor.Width);
        }

        private void pictureBox2_BindHeight(object sender, Firefly.Box.UI.Advanced.IntBindingEventArgs e)
        {
            e.Value = (int) (_controller.GetNewHeight()*ScalingFactor.Height);
        }

        private void pictureBox1_BindLeft(object sender, Firefly.Box.UI.Advanced.IntBindingEventArgs e)
        {
            e.Value = (int) ((121 - _controller.GetOriginalWidth()/2)* ScalingFactor.Width);
        }

        private void pictureBox1_BindTop(object sender, Firefly.Box.UI.Advanced.IntBindingEventArgs e)
        {
            e.Value = (int) ((164 - _controller.GetOriginalHeight()/2) * ScalingFactor.Height);
        }

        private void pictureBox2_BindLeft(object sender, Firefly.Box.UI.Advanced.IntBindingEventArgs e)
        {
            e.Value = (int) ((337 - _controller.GetNewWidth()/2)*ScalingFactor.Width);
        }

        private void pictureBox2_BindTop(object sender, Firefly.Box.UI.Advanced.IntBindingEventArgs e)
        {
            e.Value = (int) ((164 - _controller.GetNewHeight()/2)*ScalingFactor.Height);
        }
    }

    

    

    

}
