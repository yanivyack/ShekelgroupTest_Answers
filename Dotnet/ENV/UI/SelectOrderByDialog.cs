using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ENV.Data;
using ENV.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;
using Entity = ENV.Data.Entity;

namespace ENV.UI
{
    public partial class SelectOrderByDialog : System.Windows.Forms.Form
    {
        public SelectOrderByDialog()
        {
            InitializeComponent();
            Ok.Text = ENV.LocalizationInfo.Current.Ok;
            Cancel.Text = ENV.LocalizationInfo.Current.Cancel;

            RightToLeft = LocalizationInfo.Current.RightToLeft;
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            ItemSelect();
        }

        bool _selected = false;
        void ItemSelect()
        {
            _selected = true;
            Close();
            
        }

        private void OrdersByList_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtColumns.Text = OrdersByList.SelectedItem != null ? ((ListItem) OrdersByList.SelectedItem).OrderByColumns : String.Empty;
        }


        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        public static bool EnableKBPut = JapaneseMethods.Enabled;
        public static bool DonotInsertEmptyRow = false;
        internal interface SelectOrderByParent
        {
            Firefly.Box.Data.Entity From { get; }
            Sort OrderBy { get; set; }
            Firefly.Box.Data.Advanced.ColumnCollection Columns { get;}
            string Title { get;  }

            void SaveRowAndDoAndCallBackOnOk(Action<Action<bool>> whatToDo);
        }
        internal class SelectOrderByParentBridgeToUIController : SelectOrderByParent
        {
            private UIController _task;

            public SelectOrderByParentBridgeToUIController(UIController task)
            {
                this._task = task;
            }

            public Firefly.Box.Data.Entity From { get { return _task.From; } }
            public Sort OrderBy { get { return _task.OrderBy; } set { _task.OrderBy = value; } }

            public ColumnCollection Columns { get { return _task.Columns; } }

            public string Title { get { return _task.Title; } }

            public void SaveRowAndDoAndCallBackOnOk(Action<Action<bool>> whatToDo)
            {
                _task.SaveRowAndDo(o => { whatToDo((positionToRow) => { o.ReloadData(positionToRow);
                    if (!positionToRow)
                        o.GoToFirstRow();
                        }); });
            }
        }
        public void ShowDialog(System.Windows.Forms.Form parent, UIController task)
        {
            ShowDialog(parent, new SelectOrderByParentBridgeToUIController(task));
        }
        internal void ShowDialog(System.Windows.Forms.Form parent, SelectOrderByParent task)
        {
            Init(task);
            if (task.From == null) return;

            task.SaveRowAndDoAndCallBackOnOk(
                reloadData =>
                {
                    _selected = false;
                    if (EnableKBPut)
                    {
                        var indexes = new Entity("Indexes", LocalizationInfo.Current.SortByIndex, new MemoryDatabase());
                        var number = new NumberColumn("Number", "5Z", "#");
                        var indexName = new TextColumn("Index Name", "50", LocalizationInfo.Current.Name);
                        var selectedItem = new NumberColumn();
                        indexes.Columns.Add(number, indexName);
                        indexes.SetPrimaryKey(number);

                        int i = 0;
                        if (!DonotInsertEmptyRow)
                            indexes.Insert(() => number.Value = 0);

                        foreach (ListItem item in OrdersByList.Items)
                        {
                            indexes.Insert(() =>
                                {
                                    number.Value = ++i;
                                    indexName.Value = item.OrderByCommand.Caption;
                                    if (item.OrderByCommand == task.OrderBy)
                                        selectedItem.Value = number.Value;
                                }
                            );
                        }

                        if (selectedItem == 0)
                            selectedItem.Value = 1;

                        Utilities.SelectionList.Show(selectedItem, indexes, number, indexName);

                        if (selectedItem.Value > 0)
                        {
                            var x = task.OrderBy.Reversed;
                            task.OrderBy = ((ListItem) OrdersByList.Items[selectedItem - 1]).OrderByCommand; 
                            if (x)
                                task.OrderBy.Reversed = true;
                            reloadData(true);
                        }
                    }
                    else
                    {
                        Firefly.Box.Context.Current.InvokeUICommand(() => ShowDialog(parent));
                        if (_selected)
                        {
                            var x = task.OrderBy.Reversed;
                            task.OrderBy = ((ListItem) OrdersByList.SelectedItem).OrderByCommand;
                            if (x)
                                task.OrderBy.Reversed = true;
                            reloadData(true);
                        }
                    }
                });
            
        }

        void Init(SelectOrderByParent task)
        {
            Entity from = task.From as Entity;
            if (from == null || from.Indexes.Count == 0)
                return;

            this.Text = ENV.LocalizationInfo.Current.SortByIndex + " " + from.Caption;
            bool initiated = OrdersByList.Items.Count > 0;

            OrdersByList.SelectedIndex = -1;
            bool foundTaskOrderBy = false;
            int i = 0;
            foreach (var index in from.Indexes)
            {
                if (!initiated)
                    OrdersByList.Items.Add(new ListItem(index));

                if (task.OrderBy == index)
                {
                    OrdersByList.SelectedIndex = i;
                    foundTaskOrderBy = true;
                }
                i++;
            }
            if (!foundTaskOrderBy)
            {
                if (task.OrderBy.Caption == "")
                    task.OrderBy.Caption = LocalizationInfo.Current.Sort;

                var lastIndex = (ListItem)OrdersByList.Items[OrdersByList.Items.Count -1];
                var hasSort = lastIndex.OrderByCommand.Caption == LocalizationInfo.Current.Sort;
                if (hasSort)
                    OrdersByList.Items.Remove(lastIndex);
                OrdersByList.Items.Add(new ListItem(task.OrderBy));

                OrdersByList.SelectedIndex = OrdersByList.Items.Count - 1;
            }
            if (OrdersByList.SelectedIndex == -1)
                OrdersByList.SelectedIndex = 0;
        }


        class ListItem
        {
            Sort _orderByCommand;
            string _orderByColumns;

            public ListItem(Sort orderByCommand)
            {
                _orderByCommand = orderByCommand;
                _orderByColumns = "";
                bool first = true;
                foreach (SortSegment segment in orderByCommand.Segments)
                {
                    if(first)
                        first = false;
                    else
                        _orderByColumns += "\r\n";
                    _orderByColumns += segment.Column.Caption;
                }
            }

            public Sort OrderByCommand
            {
                get { return _orderByCommand; }
            }
            public override string ToString()
            {
                return _orderByCommand.Caption;
            }

            public string OrderByColumns
            {
                get { return _orderByColumns; }
            }
        }

        private void OrdersByList_DoubleClick(object sender, EventArgs e)
        {
            ItemSelect();
        }

        private void SelectOrderByDialog_Load(object sender, EventArgs e)
        {
            this.OrdersByList.Focus();
        }

    }
}