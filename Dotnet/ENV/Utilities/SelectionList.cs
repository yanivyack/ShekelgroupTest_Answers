using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firefly.Box.Data.Advanced;
using Firefly.Box;

namespace ENV.Utilities
{
    public class SelectionList
    {
        public static void Show<T>(TypedColumnBase<T> dataColumn,
            ENV.Data.Entity e,
            TypedColumnBase<T> valueColumn,
            params ColumnBase[] descriptionColumns
            )
        {
            new SelectionListClass<T>(e, valueColumn, descriptionColumns).Run(dataColumn);
        }
        public static void Show<T>(TypedColumnBase<T> dataColumn,
           ENV.Data.Entity e,
           TypedColumnBase<T> valueColumn,
           Sort orderBy,
           params ColumnBase[] descriptionColumns
           )
        {
            var r = new SelectionListClass<T>(e, valueColumn, descriptionColumns);
            if (orderBy != null)
                r.OrderBy = orderBy;
            r.Run(dataColumn);
        }
        public static void Show<T>(TypedColumnBase<T> dataColumn,
            ENV.Data.Entity e,
            TypedColumnBase<T> valueColumn, FilterBase staticFilter,
            params ColumnBase[] descriptionColumns
            )
        {
            var s = new SelectionListClass<T>(e, valueColumn, descriptionColumns);
            if (staticFilter != null)
                s.Where.Add(staticFilter);
            s.Run(dataColumn);
        }

        class SelectionListClass<T> : UIControllerBase
        {
            TypedColumnBase<T> _valueColumn;
            TypedColumnBase<T> _dataColumn;

            public SelectionListClass(ENV.Data.Entity e,
                TypedColumnBase<T> valueColumn,
                ColumnBase[] descriptionColumns)
            {
                _valueColumn = valueColumn;
                From = e;


                AllowUpdate = false;
                AllowInsert = false;
                AllowDelete = false;
                AllowSelect = true;
                Activity = Firefly.Box.Activities.Browse;

                View = () =>
                {
                    var t = new ENV.UI.Form
                    {
                        StartPosition = Firefly.Box.UI.WindowStartPosition.CenterMDI,
                        BackColor = Labs.FaceLiftDemo.BackGroundColor
                    };
                    t.Text = e.Caption;
                    var g = new ENV.UI.Grid
                    {
                        Left = 5,
                        Top = 5,
                        Height = 350,
                        UnderConstructionNewGridLook = true,
                        EnableGridEnhancementsCodeSample = true
                    };
                    t.Controls.Add(g);
                    var l = new List<ColumnBase>();
                    l.Add(valueColumn);
                    l.AddRange(descriptionColumns);
                    foreach (var item in l)
                    {
                        var gc = new ENV.UI.GridColumn();
                        gc.Text = item.Caption;
                        var tb = new ENV.UI.TextBox { Left = 2, Top = 2, Data = item, Style = Firefly.Box.UI.ControlStyle.Flat };
                        tb.ResizeToFit(Math.Min(25, item.FormatInfo.MaxLength == 0 ? 25 : item.FormatInfo.MaxLength));
                        gc.Controls.Add(tb);
                        gc.ResizeToFitContent();
                        g.Controls.Add(gc);
                    }
                    g.ResizeToFitContent();

                    var okb = new Labs.UI.Button
                    {
                        Text = LocalizationInfo.Current.Choose,
                        Top = g.Bottom + 5
                    };
                    okb.Left = g.Right - okb.Width;
                    okb.Click += (s, ee) => ee.Raise(Firefly.Box.Command.Select);
                    t.Controls.Add(okb);
                    t.AcceptButton = okb;
                    t.ClientSize = new System.Drawing.Size(g.Right + 5, okb.Bottom + 5);
                    return t;
                };


            }

            protected override void OnSavingRow()
            {
                if (u.KBGet(1) == Firefly.Box.Command.Select)
                    _dataColumn.Value = _valueColumn.Value;
            }

            public void Run(TypedColumnBase<T> dataColumn)
            {
                _dataColumn = dataColumn;
                StartOnRowWhere.Clear();
                StartOnRowWhere.Add(_valueColumn.IsEqualTo(dataColumn));
                Execute();

            }
        }

    }
}