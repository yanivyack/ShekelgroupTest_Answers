using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using ENV.Data.DataProvider;


namespace ENV.Data
{

    public class InnerSelectHelper
    {
        public void TurnToCount(NumberColumn column, Entity childEntity, FilterBase where)
        {
            ApplyTo("count (*)", column, childEntity, where);
        }
        public void TurnToSum(NumberColumn column, NumberColumn childColumn, FilterBase where)
        {
            ApplyTo("sum (" + childColumn.Name + ")", column, childColumn.Entity, where);
        }
        public void TurnToGetValue<T>(TypedColumnBase<T> column, TypedColumnBase<T> childColumn, FilterBase where, Sort orderBy = null)
        {
            ApplyTo("top 1 " + childColumn.Name, column, childColumn.Entity, where, orderBy);
        }
        public void TurnToExist(BoolColumn column, Entity childEntity, FilterBase where)
        {
            ApplyTo("max (1)", column, childEntity, where);
        }

        public class GetValueHelperClass
        {
            private FilterBase _where;
            private Sort _orderBy;
            private InnerSelectHelper _parent;

            public GetValueHelperClass(FilterBase where, Sort orderBy, InnerSelectHelper parent)
            {
                this._where = where;
                this._orderBy = orderBy;
                this._parent = parent;
            }

            public void TurnToGetValue<T>(TypedColumnBase<T> column, TypedColumnBase<T> childColumn)
            {
                _parent.ApplyTo("top 1 " + childColumn.Name, column, childColumn.Entity, _where, _orderBy);
            }
        }

        public GetValueHelperClass GetValueHelper(FilterBase where, Sort orderBy = null)
        {
            return new GetValueHelperClass(where, orderBy, this);
        }


        void ApplyTo(string agregateStatement, ColumnBase resultColumn, Firefly.Box.Data.Entity childEntity, FilterBase where, Sort orderBy = null)
        {


            resultColumn.Name = GetSQL(agregateStatement, childEntity, where, orderBy);
            resultColumn.DbReadOnly = true;
            _controller.From.Columns.Add(resultColumn);
        }


        List<Firefly.Box.Data.Entity> _relevantEntities = new List<Firefly.Box.Data.Entity>();
        Dictionary<Firefly.Box.Data.Entity, string> _aliases = new Dictionary<Firefly.Box.Data.Entity, string>();
        bool _isOracle;
        ControllerBase _controller;
        public InnerSelectHelper(ControllerBase controller)
        {
            _controller = controller;
            var envEntity = controller.From as ENV.Data.Entity;
            if (envEntity != null)
            {
                var dp = envEntity.DataProvider as DynamicSQLSupportingDataProvider;
                if (dp != null)
                    _isOracle = dp.IsOracle;
            }

            _relevantEntities.Add(controller.From);
            foreach (var item in controller.GetRelations())
            {
                if (item.Type == RelationType.Join || item.Type == RelationType.OuterJoin)
                {
                    if (_aliases.Count == 0)
                    {
                        _aliases.Add(controller.From, "A");
                    }
                    _aliases.Add(item.From, ((char)('A' + _aliases.Count)).ToString());
                    _relevantEntities.Add(item.From);
                }
            }
            if (_aliases.Count == 0)
                _aliases.Add(controller.From, Entity.GetEntityName(controller.From));



        }


        //public readonly Firefly.Box.Data.Advanced.FilterCollection Where = new Firefly.Box.Data.Advanced.FilterCollection();

        string GetSQL(string agregateStatement, Firefly.Box.Data.Entity innerEntity, FilterBase whereFilter, Sort orderBy)
        {
            var _select = @"isnull((
                            select " + agregateStatement + @" 
                            from " + Entity.GetEntityName(innerEntity);
            string where = "";

            var re = new List<Firefly.Box.Data.Entity>(_relevantEntities);
            re.Add(innerEntity);
            if (whereFilter != null)
            {
                var x = FilterBase.GetIFilter(whereFilter, false, re.ToArray());
                var p = new NoParametersFilterItemSaver(true, _isOracle ? OracleClientEntityDataProvider.DateTimeStringFormat : SQLClientEntityDataProvider.DateTimeStringFormat, DummyDateTimeCollector.Instance, _isOracle ? OracleClientEntityDataProvider.DateTimeStringFormatForToString : null);
                var z = new SQLFilterConsumer(
                        p,

                        y =>
                        {
                            string s;
                            if (_aliases.TryGetValue(y.Entity, out s))
                                return s + "." + y.Name;

                            else if (y.Entity == innerEntity)
                                return y.Name;
                            throw new InvalidOperationException("Only expected columns from main table or inner table");
                        }, false, new dummySqlFilterHelper(p))
                { NewLinePrefix = "" };
                x.AddTo(z);
                where = " where " + z.Result.ToString();
            }
            if (orderBy != null && orderBy.Segments.Count > 0)
            {
                string s = "";
                foreach (var col in orderBy.Segments)
                {
                    if (s.Length > 0)
                        s += ", ";
                    s += col.Column.Name;
                    var dir = col.Direction == SortDirection.Descending;
                    if (orderBy.Reversed)
                        dir = !dir;
                    if (dir)
                        s += " desc";
                }
                where += " Order By " + s;
            }
            return "=" + _select + where + "),0)";
        }


    }
    public class OracleInnerSelectHelper
    {
        Firefly.Box.Data.Entity _from;

        public OracleInnerSelectHelper(Firefly.Box.Data.Entity From)
        {
            _from = From;
        }
        public void TurnToGetValue<T>(TypedColumnBase<T> column, TypedColumnBase<T> childColumn, FilterBase where)
        {
            column.Name = "(select " + childColumn.Name +
                    " from " + Entity.GetEntityName(childColumn.Entity)+
                    " where " + ENV.Utilities.FilterHelper.ToSQLWhere(where, false, _from, childColumn.Entity) +
                    " fetch next 1 row only)";
            if (column is TextColumn)
            {
                column.Name = "nvl(" + column.Name + ", ' ')";
            }
            column.DbReadOnly = true;
            _from.Columns.Add(column);

        }
    }

}
