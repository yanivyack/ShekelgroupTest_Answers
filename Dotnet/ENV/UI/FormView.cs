using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ENV.Data;
using Firefly.Box;
using Firefly.Box.Data.Advanced;

namespace ENV.UI
{
    public class FormView : ENV.UI.Form
    {
        int padding = 5;
        int _top;
        List<ENV.UI.Label> _labels = new List<ENV.UI.Label>();
        List<System.Windows.Forms.Control> _textboxes = new List<System.Windows.Forms.Control>();
        int _labelWidth;
        int _textBoxWidth;
        int _actionsOffset = 0;
        public FormView(params ColumnBase[] columns)
        {
            StartPosition = Firefly.Box.UI.WindowStartPosition.CenterMDI;
            Width = 200;
            RightToLeftLayout = true;
            BackColor = ENV.Labs.FaceLiftDemo.BackGroundColor;
            _top = padding;
            AddColumns(columns);
        }
        public void AddAction(string name, Action what)
        {
            if (_top < 120)
                _top = 120;
            ClientSize = new Size(Math.Max(ClientSize.Width, 300), ClientSize.Height);
            var tc = new TextColumn();
            tc.DefaultValue = name;
            var b = new ENV.UI.Button { Data = tc, Top = _top, CoolEnabled = false };
            b.FontScheme = new Firefly.Box.UI.FontScheme() { Font = b.Font };
            var w = b.Width;
            b.ResizeToFit(name);
            if (w > b.Width)
                b.Width = w;
            b.Left = ClientSize.Width - padding - b.Width - _actionsOffset;
            _actionsOffset += b.Width + padding;
            b.Click += delegate { what(); };

            AcceptButton = b;
            ResizeToFit(b);
            Controls.Add(b);
            if (b.Left < padding)
            {
                var y = padding - b.Left;
                foreach (var item in Controls)
                {
                    var btn = item as Button;
                    if (btn != null)
                    {
                        btn.Left += y;
                        ResizeToFit(btn);
                    }

                }
            }
        }

        public void AddCombo<T>(TypedColumnBase<T> column, Entity sourceEntity = null, TypedColumnBase<T> valueColumn = null, TextColumn displayColumn = null, Sort orderBy = null, FilterBase where = null, string values = null, string displayValues = null)
        {
            var c = new ENV.UI.ComboBox() { Data = column };
            if (sourceEntity != null)
            {
                c.ListSource = sourceEntity;
                c.ValueColumn = valueColumn;
                if (displayColumn != null)
                    c.DisplayColumn = displayColumn;
                else
                    c.DisplayColumn = valueColumn;
                if (where != null)
                    c.ListWhere.Add(where);
                if (orderBy != null)
                    c.ListOrderBy = orderBy;
                else
                    c.ListOrderBy.Add(valueColumn);
            }
            if (values != null)
                c.Values = values;
            if (displayValues != null)
                c.DisplayValues = displayValues;
            AddColumn(column.Caption, c);
        }
        public void AddCheckBox(Data.BoolColumn column)
        {
            var cb = new CheckBox();
            cb.Data = column;
            cb.UseColumnInputRangeWhenEmptyText = false;
            AddColumn(column.Caption, cb);
        }
        public void AddColumn(string name, Control displayControl)
        {

            var label = new ENV.UI.Label
            {
                Style = Firefly.Box.UI.ControlStyle.Flat,
                Width = 150,
                Left = padding,
                Top = _top,
                Text = name
            };
            label.FontScheme = new Firefly.Box.UI.FontScheme() { Font = label.Font };
            label.ResizeToFitContent();
            if (label.Width <= _labelWidth)
            {
                label.Width = _labelWidth;
            }
            else
            {
                foreach (var label1 in _labels)
                {
                    label1.Width = _labelWidth;
                }
                foreach (var textBox in _textboxes)
                {
                    textBox.Left = label.Right + padding;
                }
                _labelWidth = label.Width;
            }
            _labels.Add(label);
            Controls.Add(label);
            var tb = displayControl;

            tb.Left = label.Right + padding;
            tb.Top = _top;
            if (tb.Height<label.Height)
                tb.Height = label.Height;

            if (tb.Width < _textBoxWidth)
            {
                tb.Width = _textBoxWidth;
            }
            else
            {
                foreach (var textBox in _textboxes)
                {
                    textBox.Width = tb.Width;
                }
                _textBoxWidth = tb.Width;
            }

            _textboxes.Add(tb);
            ResizeToFit(tb);
            Controls.Add(tb);

            _top = label.Bottom + padding;
        }
        public void AddColumns(params ColumnBase[] args)
        {
            foreach (var item in args)
            {
                AddColumn(item);
            }
        }
        public void AddColumn(BoolColumn column)
        {
            AddColumn(column.Caption, new Firefly.Box.UI.CheckBox { Data = column, Text = "" });
        }
        public void AddColumn(ColumnBase column)
        {
            var control = GridView.GetControlBasedOnControlTypeInColumn(column, x => x.ControlType);
            if (control == null)
            {
                var tb = new TextBox { Data = column };
                tb.FontScheme = new Firefly.Box.UI.FontScheme() { Font = tb.Font };
                tb.ResizeToFit(Math.Max(30, Math.Min(column.FormatInfo.MaxLength, 50)));
                control = tb;
            }
            AddColumn(column.Caption, control);
        }
        public void AddPassword(TextColumn column)
        {
            var tb = new TextBox { Data = column, UseSystemPasswordChar = true };
            tb.FontScheme = new Firefly.Box.UI.FontScheme() { Font = tb.Font };
            tb.ResizeToFit(Math.Max(30, Math.Min(column.FormatInfo.MaxLength, 50)));
            AddColumn(column.Caption, tb);
        }
        void ResizeToFit(Control c)
        {
            ClientSize = new Size(Math.Max(ClientSize.Width, c.Right + padding), c.Bottom + padding);
        }
    }
}