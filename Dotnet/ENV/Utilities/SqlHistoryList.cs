using System;
using System.Windows.Forms;
using ENV.Data;

namespace ENV.Utilities
{
    internal partial class SqlHistoryList : Form
    {
        readonly TextColumn _selectedValue;

        public SqlHistoryList(SQLHistory data, TextColumn selectedValue, bool topMost)
        {
            InitializeComponent();
            _selectedValue = selectedValue;
            TopMost = topMost;
            this.listView1.View = View.Details;
            data.ForEachRow(() =>
            {
                listView1.Items.Add(new ListViewItem(new[] {data.LastRun.ToString(), data.SQL.ToString()}));
            });
        }

        void listView1_Click(object sender, EventArgs e)
        {
            MakeSelection();
        }

        void MakeSelection()
        {
            var selectedSql = listView1.SelectedItems[0].SubItems[1].Text;
            _selectedValue.Value = selectedSql;
            this.Close();
        }

        void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
                MakeSelection();
        }

        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
