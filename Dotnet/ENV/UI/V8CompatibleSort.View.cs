using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Firefly.Box;
using Firefly.Box.UI.Advanced;
using ENV;

namespace ENV.UI
{
    partial class V8CompatibleSortView : UI.Form
    {
        V8CompatibleSort _controller;
        public V8CompatibleSortView(V8CompatibleSort controller)
        {
            _controller = controller;
            InitializeComponent();
            RightToLeft = LocalizationInfo.Current.RightToLeft;
            gcLetter.Text = LocalizationInfo.Current.Field;
            gcDescription.Text = LocalizationInfo.Current.FieldName;
            okButton.Text = LocalizationInfo.Current.Ok;
            cancelButton.Text = LocalizationInfo.Current.Cancel;

        }

        private void txtLetter_BindReadOnly(object sender, BooleanBindingEventArgs e)
        {
            e.Value = _controller._sc.Order == 0;
        }

        private void button2_Click(object sender, ButtonClickEventArgs e)
        {
            _controller.Ok();
        }

        private void button1_Click(object sender, ButtonClickEventArgs e)
        {
            Close();
        }
    }
}