using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ENV.Data.DataProvider;
using Firefly.Box;
using ENV.Data;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;

namespace ENV.Labs
{
    class CustomFilterDialog<T> : UIControllerBase
    {
        FilterBase _result;
        TypedColumnBase<T>[] _filteredColumn;
        TypedColumnBase<T> _fromColumn;
        TypedColumnBase<T> _toColumn;


        internal readonly TextColumn FilterType = new TextColumn() { Format = "2" };
        internal readonly BoolColumn IsCaseSensitive = new BoolColumn();

        UI.CustomFilterDialogUI _form;
        UIController _uic;
        public CustomFilterDialog(TypedColumnBase<T> fromColumn, TypedColumnBase<T> tocolumn, UIController uic, params  TypedColumnBase<T>[] filteredColumn)
        {
            _uic = uic;

            View = () => _form = new UI.CustomFilterDialogUI(ApplyFilter);

            _filteredColumn = filteredColumn;
            _fromColumn = fromColumn;
            _toColumn = tocolumn;
            Columns.Add(FilterType, _fromColumn, _toColumn, IsCaseSensitive);
            _form.filterTypeComboBox.Data = FilterType;
            _form.fromValueTextBox.Data = _fromColumn;
            _form.toValueTextBox.Data = _toColumn;
            _form.isCaseSensitiveCheckBox.Data = IsCaseSensitive;
            if (_filteredColumn.Length == 1)
            {
                fromColumn.Format = _filteredColumn[0].Format;
                tocolumn.Format = _filteredColumn[0].Format;
                _form.messageLabel.Text = String.Format("Show Rows Where {0}:", _filteredColumn[0].Caption);
            }
            _form.filterTypeComboBox.Values = "=,>,<,><";
            _form.filterTypeComboBox.DisplayValues = "Is Equal To,Is Greater Than,Is Less Than,Is Between";
            FilterType.DefaultValue = "=";
            if (typeof(T) == typeof(Text))
            {
                _form.filterTypeComboBox.Values = "%," + _form.filterTypeComboBox.Values;
                _form.filterTypeComboBox.DisplayValues = "Contains," + _form.filterTypeComboBox.DisplayValues;
                _form.isCaseSensitiveCheckBox.Visible = true;
                FilterType.DefaultValue = "%";
            }

        }

        public FilterBase Run()
        {
            Execute();
            return _result;
        }

        public void ApplyFilter()
        {

            _result = null;
            Action<FilterBase> addFilter = null;
            addFilter = x =>
            {
                _result = x;
                addFilter = y => _result = _result.Or(y);
            };
            foreach (var ffc in _filteredColumn)
            {
                var filterColumn = ffc;
                T filterValue = _fromColumn;
                T toValue = _toColumn;
                switch (FilterType.Trim())
                {
                    case "%":
                        var fc = new FilterCollection();
                        {
                            if (filterColumn.Entity == _uic.From)
                            {
                                var e = filterColumn.Entity as ENV.Data.Entity;
                                if (e!=null&& e.DataProvider is DynamicSQLSupportingDataProvider)
                                {
                                    if (IsCaseSensitive)
                                    {
                                        fc.Add("{0} like {1}", filterColumn, '%' + filterValue.ToString().TrimEnd(' ') + '%');
                                    }
                                    else
                                        fc.Add("upper({0}) like {1}", filterColumn, '%' + filterValue.ToString().TrimEnd(' ').ToUpper() + '%');
                                    addFilter(fc);
                                    break;
                                }
                            }
                        }
                        if (IsCaseSensitive)
                            fc.Add(
                                () => filterColumn.Value != null && filterColumn.ToString().IndexOf(filterValue.ToString().Trim()) >= 0);
                        else
                            fc.Add(
                                () =>
                                filterColumn.Value != null &&
                                filterColumn.ToString().ToUpper(CultureInfo.InvariantCulture).IndexOf(filterValue.ToString().Trim().ToUpper(CultureInfo.InvariantCulture)) >= 0);
                        addFilter(fc);
                        break;
                    case "=":
                        addFilter(filterColumn.IsEqualTo(filterValue));
                        break;
                    case ">":
                        addFilter(filterColumn.IsGreaterThan(filterValue));
                        break;
                    case "<":
                        addFilter(filterColumn.IsLessThan(filterValue));
                        break;
                    case "><":
                        addFilter(filterColumn.IsBetween(filterValue, toValue));
                        break;
                }

            }


        }
    }
}
