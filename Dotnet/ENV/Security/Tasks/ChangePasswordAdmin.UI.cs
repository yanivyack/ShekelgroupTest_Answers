#region Copyright Firefly Ltd 2009
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Firefly.Box;
using Firefly.Box.UI.Advanced;

namespace ENV.Security.Tasks
{
    partial class ChangePasswordAdminUI : UI.Form
    {
        ChangePassword _task;
        public ChangePasswordAdminUI(ChangePassword task)
        {
            _task = task;
            InitializeComponent();
            this.button1.Format = LocalizationInfo.Current.Exit;
            this.textBox4.Text = LocalizationInfo.Current.NewPassword;
            this.textBox6.Text = LocalizationInfo.Current.ConfirmNewPassword;
            this.button2.Format = LocalizationInfo.Current.Ok;
            this.Text = LocalizationInfo.Current.ChangePassword;
        }

        private void button2_Click(object sender, ButtonClickEventArgs e)
        {
            _task.TryChangePassword();
        }

        private void button1_Click(object sender, ButtonClickEventArgs e)
        {
            e.Raise(Command.Exit);
        }
    }
}