using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ENV.Labs;
using Firefly.Box;
using Firefly.Box.UI.Advanced;
using ENV;

namespace ENV.Data.DataProvider.Views
{
    partial class SqlReplaceManagerView : ENV.UI.Form
    {
        SqlReplaceManager _controller;
        public SqlReplaceManagerView(SqlReplaceManager controller)
        {
            _controller = controller;
            InitializeComponent();
            BackColor = FaceLiftDemo.BackGroundColor;
            tabControl1.UseVisualStyles = true;
            tabControl1.BackColor = FaceLiftDemo.BackGroundColor;
        }

        private void showMergedSQLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _controller.ShowMerged();
        }

        private void runAndCompareSQLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _controller.RunSQL();
        }
    }
}