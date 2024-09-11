using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Firefly.Box.UI;
using System.ComponentModel;
using Firefly.Box.Data.Advanced;
using ENV.Utilities;

namespace ENV.Printing
{
    public class Grid:Firefly.Box.UI.Grid
    {
        public Grid()
        {
            Style = Firefly.Box.UI.ControlStyle.Flat;
            GridColumnType = typeof (GridColumn);
            DefaultTextBoxType = typeof (TextBox);
            ColorScheme = new ColorScheme(Color.Black, Color.White);
        }
        protected override string Translate(string term)
        {
            return base.Translate(ENV.Languages.Translate(term));
        }
        double _rowHeight = 0;
        public new double RowHeight
        {
            get
            {
                if (_rowHeight <= 0)
                    return base.RowHeight;
                return _rowHeight;
            }
            set
            {
                _rowHeight = value;
                var x = (int)_rowHeight;
                if (x == base.RowHeight)
                    base.RowHeight++;
                base.RowHeight = x;

            }
        }
        protected override Type GetControlTypeForWizard(ColumnBase column)
        {
            var col = column as IENVColumn;
            if (col != null && col.ControlTypePrintingOnGrid != null)
                return col.ControlTypePrintingOnGrid;
            return base.GetControlTypeForWizard(column);
        }
        protected override double GetExactRowHeight()
        {
            return RowHeight;
        }

        [Browsable(false)]
        public int LeftForBindLeftOfControlsOnGrid
        {
            get
            {
                return DeferredLeft;
            }
        }
    }
}
