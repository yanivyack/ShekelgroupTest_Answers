using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;
using System.Reflection;
using System.Collections;

namespace ENV.UI
{
    public partial class CustomOrderByDialog : System.Windows.Forms.Form
    {
        public CustomOrderByDialog()
        {
            InitializeComponent();
            this.Ok.Text = ENV.LocalizationInfo.Current.Ok;
            this.Cancel.Text = ENV.LocalizationInfo.Current.Cancel;
            Text = ENV.LocalizationInfo.Current.Sort;
            RightToLeft = LocalizationInfo.Current.RightToLeft;
            CancelButton = Cancel;


        }

        bool _ok;
        private void Ok_Click(object sender, EventArgs e)
        {
            _ok = true;
            Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        public static bool V8ComplatibleDialog { get; set; }

        SelectOrderByDialog.SelectOrderByParent _task;
        public void ShowDialog(System.Windows.Forms.Form parent, Firefly.Box.UIController task)
        {
            ShowDialog(parent, new SelectOrderByDialog.SelectOrderByParentBridgeToUIController(task));
        }
        public static bool PartOnFirstRowOnSort = false;
        internal void ShowDialog(System.Windows.Forms.Form parent, SelectOrderByDialog.SelectOrderByParent task)
        {
            if (_task == null)
                Init(task);
            _task = task;
            SetButtonsEnabeld();
            _ok = false;
            _task.SaveRowAndDoAndCallBackOnOk(
                delegate(Action<bool> reloadData)
                {
                    if (V8ComplatibleDialog)
                    {
                        new V8CompatibleSort().Run(task,x=>reloadData(!PartOnFirstRowOnSort));
                        return;
                    }


                    Firefly.Box.Context.Current.InvokeUICommand(()=>ShowDialog(parent));
                    if (!_ok)
                        return;
                    
                    int i = 0;
                    var s = new Sort();
                    foreach (object item in SelectedColumns.Items)
                    {
                        var x = item as ListItem;
                        if (x != null)
                            s.Add(x.Column, x.Dir);
                        

                    }
                    _task.OrderBy = s;
                    reloadData(!PartOnFirstRowOnSort);
                    
                });
            
        }
        void Init(SelectOrderByDialog.SelectOrderByParent task)
        {
            ColumnsList.Items.Clear();
            foreach (ColumnBase column in task.Columns)
            {
                if (!string.IsNullOrEmpty(column.Caption))
                {
                    var li = new ListItem(column);
                    ColumnsList.Items.Add(li);
                }
            }
            ColumnsList.SelectedIndex = 0;

            foreach (var item in task.From.Indexes)
            {
                if (item == task.OrderBy)
                    return;
            }
            
            foreach (var seg in task.OrderBy.Segments)
            {
                foreach (var item in new ArrayList( ColumnsList.Items))
                {
                    var li = item as ListItem;
                    if (li != null && li.Column == seg.Column)
                    {
                        li.Dir = seg.Direction;
                        ColumnsList.SelectedIndices.Clear();
                        ColumnsList.SelectedItems.Add(li);
                        AddItem();
                        continue;
                    }
                }
            }
            
            

        }

        class ListItem
        {
            ColumnBase _Column;

            public ListItem(ColumnBase column)
            {
                _Column = column;
                Dir = SortDirection.Ascending;
            }

            public ColumnBase Column
            {
                get { return _Column; }
            }
            public SortDirection Dir;
            public override string ToString()
            {
                return _Column.Caption+(Dir==  SortDirection.Descending?" descending":"");
            }
        }

        private void Add_Click(object sender, EventArgs e)
        {
            AddItem();
        }

        private void ColumnsList_DoubleClick(object sender, EventArgs e)
        {
            AddItem();
        }

        void AddItem()
        {
            MoveItemsBettwenLists(ColumnsList, SelectedColumns);
        }

        void MoveItemsBettwenLists(System.Windows.Forms.ListBox source, System.Windows.Forms.ListBox destinition)
        {
            List<ListItem> list = new List<ListItem>();
            foreach (object item in source.SelectedItems)
            {
                list.Add((ListItem)item);
            }
            foreach (ListItem listItem in list)
            {
                source.Items.Remove(listItem);
                destinition.Items.Add(listItem);
            }

            SetButtonsEnabeld();
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            RemoveItem();
        }
        private void SelectedColumns_DoubleClick(object sender, EventArgs e)
        {
            RemoveItem();
        }

        void RemoveItem()
        {
            MoveItemsBettwenLists(SelectedColumns, ColumnsList);
        }


        private void Up_Click(object sender, EventArgs e)
        {
            SetSortOrder(-1);
        }

        private void Down_Click(object sender, EventArgs e)
        {
            SetSortOrder(1);
        }

        void SetSortOrder(int sortIdent)
        {
            int index = SelectedColumns.SelectedIndex;
            ListItem item = (ListItem)SelectedColumns.SelectedItem;
            SelectedColumns.Items.Remove(item);
            SelectedColumns.Items.Insert(index + sortIdent, item);
            SelectedColumns.SelectedIndex = index + sortIdent;
            SetButtonsEnabeld();
        }

        void SetButtonsEnabeld()
        {
            Up.Enabled = SelectedColumns.SelectedIndex > 0;
            Down.Enabled = SelectedColumns.SelectedIndex < SelectedColumns.Items.Count - 1 && SelectedColumns.SelectedIndex > -1;
            Add.Enabled = ColumnsList.SelectedItems.Count > 0;
            Remove.Enabled = SelectedColumns.SelectedItems.Count > 0;
            AscDesc.Enabled = SelectedColumns.SelectedItems.Count > 0;
            if (AscDesc.Enabled)
            {
                if (((ListItem)SelectedColumns.SelectedItems[0]).Dir == SortDirection.Ascending)
                    AscDesc.Text = "D";
                else
                    AscDesc.Text = "A";
            }
        }

        private void SelectedColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetButtonsEnabeld();
        }

        private void ColumnsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetButtonsEnabeld();
        }

        private void AscDesc_Click(object sender, EventArgs e)
        {

            foreach (var item in SelectedColumns.SelectedItems)
            {
                var l = item as ListItem;
                if (l != null)
                {
                    if (AscDesc.Text == "D")
                        l.Dir = SortDirection.Descending;
                    else
                        l.Dir = SortDirection.Ascending;
                }
            }
            SelectedColumns.RefreshItems();
           


            SetButtonsEnabeld();
        }
    }
    class myListBox : System.Windows.Forms.ListBox
    {
        public new void RefreshItems()
        {
            base.RefreshItems();
        }
    }
}