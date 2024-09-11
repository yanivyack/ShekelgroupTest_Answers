using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ENV.UI
{
     partial class V8CompatibleSortSelectColumns : Form
    {
        V8CompatibleSort.V8CompatibleSortSelectColumn _controller;
        public V8CompatibleSortSelectColumns(V8CompatibleSort. V8CompatibleSortSelectColumn controller)
        {
            _controller = controller;
            InitializeComponent();
            RightToLeft = LocalizationInfo.Current.RightToLeft;
            gcDescription.Text = LocalizationInfo.Current.Name;
            gcFromFile.Text = LocalizationInfo.Current.FromEntity;
        }

        private void gcLetter_Click(object sender, EventArgs e)
        {

        }

        private void gcFromFile_Click(object sender, EventArgs e)
        {

        }
    }
}
