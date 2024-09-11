using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENV.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Data.Advanced;

namespace ENV.Data
{
    public class SQLGroupBy
    {
        StringBuilder _fromSection = new StringBuilder(), _selectSection = new StringBuilder(), _groupBySection = new StringBuilder();
        Dictionary<Firefly.Box.Data.Entity, string> _alias = new Dictionary<Firefly.Box.Data.Entity, string>();
        List<Firefly.Box.Data.Entity> _entities = new List<Firefly.Box.Data.Entity>();
        HashSet<string> _usedNames = new HashSet<string>();
        DynamicSQLEntity _entity;
        public DynamicSQLEntity Entity { get { return _entity; } }
        public SQLGroupBy(ENV.Data.Entity entity)
            : this((DynamicSQLSupportingDataProvider)entity.DataProvider)
        {
            Add(entity);
        }

        DynamicSQLSupportingDataProvider _dp;
        public SQLGroupBy(DynamicSQLSupportingDataProvider dataProvider)
        {
            _dp = dataProvider;
            _entity = new DynamicSQLEntity(dataProvider, ":1");
            _entities.Add(_entity);
            _entity.AddParameter(GenerateGroupBySQL);

        }

        string _sql = null;
        string GenerateGroupBySQL()
        {
            if (!string.IsNullOrEmpty(_sql))
                return _sql;

            using (var sw = new System.IO.StringWriter())
            {
                sw.WriteLine("Select " + _selectSection);
                sw.WriteLine("From " + _fromSection);

                var f = FilterBase.GetIFilter(_where, false, _entities.ToArray());
                string whereString = SQLFilterConsumer.DisplayFilter(f,_dp.IsOracle);
                if (whereString.Length > 0)
                    sw.WriteLine("Where " + whereString);
                if (_groupBySection.Length > 0)
                    sw.WriteLine("Group By " + _groupBySection);




                f = FilterBase.GetIFilter(_having, false, _entities.ToArray());
                string havingString = SQLFilterConsumer.DisplayFilter(f,_dp.IsOracle);
                if (!string.IsNullOrEmpty(havingString))
                    sw.WriteLine("Having " + havingString);

                if (_orderBy.Segments.Count > 0)
                {
                    bool first = true;
                    sw.Write("Order By ");
                    foreach (var seg in _orderBy.Segments)
                    {
                        if (first)
                            first = false;
                        else
                            sw.Write(", ");
                        bool decending = _orderBy.Reversed;
                        if (seg.Direction == SortDirection.Descending)
                            decending = !decending;


                        sw.Write(seg.Column.Name + (decending ? " desc" : ""));
                    }
                    sw.WriteLine();
                }
                return _sql = sw.ToString();
            }
        }

        public void Add(Firefly.Box.Data.Entity orders)
        {
            AddTo(_fromSection, string.Format("{0} {1}", ENV.Data.Entity.GetEntityName(orders), GetAlias(orders)));
        }
        string GetAlias(Firefly.Box.Data.Entity e)
        {
            if (!_alias.ContainsKey(e))
            {
                var s = e.EntityName.Split('.');
                var en = s[s.Length - 1];
                var alias = en;
                int i = 0;
                while (_usedNames.Contains(alias.ToUpper()))
                {
                    alias = en + (++i);
                }


                _alias.Add(e, alias);
                _entities.Add(e);
                foreach (var column in e.Columns)
                {
                    column.Name = alias + "." + column.Name;
                }
            }
            return _alias[e];
        }

        void AddTo(StringBuilder sb, string what)
        {
            if (sb.Length != 0)
                sb.Append(", ");
            sb.Append(what);
        }

        public void AddOuterJoin(Firefly.Box.Data.Entity e, FilterBase where)
        {
            var alias = GetAlias(e);
            var f = FilterBase.GetIFilter(where, false, _entities.ToArray());
            string whereString = SQLFilterConsumer.DisplayFilter(f,_dp.IsOracle);
            _fromSection.Append(string.Format(" left outer join {0} {1} on {2}", ENV.Data.Entity.GetEntityName(e), alias, whereString));
        }
        public void AddJoin(Firefly.Box.Data.Entity e, FilterBase where)
        {
            var alias = GetAlias(e);
            var f = FilterBase.GetIFilter(where, false, _entities.ToArray());
            string whereString = SQLFilterConsumer.DisplayFilter(f, _dp.IsOracle);
            _fromSection.Append(string.Format(" inner join {0} {1} on {2}", ENV.Data.Entity.GetEntityName(e), alias, whereString));
        }



        public void AddColumn(ColumnBase col)
        {
            _entity.Columns.Add(col);
            AddTo(_selectSection, col.Name);
            AddTo(_groupBySection, col.Name);
        }
        public void AddSumColumn(NumberColumn resultColumn, ColumnBase valueColumn)
        {
            AddBasicAggregateColumn(resultColumn, valueColumn, "sum");
        }
        public void AddCountColumn(NumberColumn resultColumn, ColumnBase valueColumn)
        {
            AddBasicAggregateColumn(resultColumn, valueColumn, "count");
        }
        public void AddCountColumn(NumberColumn resultColumn)
        {
            AddAggregateColumn(resultColumn, "count (*)");
        }
        public void AddCountDistinctColumn(NumberColumn resultColumn, ColumnBase valueColumn)
        {
            AddBasicAggregateColumn(resultColumn, valueColumn, "count distinct");
        }

        public void AddBasicAggregateColumn(NumberColumn resultColumn, ColumnBase valueColumn, string aggregateOperator)
        {
            AddAggregateColumn(resultColumn, aggregateOperator + " ({0})", valueColumn);
        }


        public void AddAggregateColumn(NumberColumn resultColumn, string operatorStringFormat, params object[] formatArgs)
        {
            if (formatArgs.Length > 0)
            {
                for (int i = 0; i < formatArgs.Length; i++)
                {
                    var y = formatArgs[i] as ColumnBase;
                    if (y != null)
                        formatArgs[i] = y.Name;
                }
                operatorStringFormat = string.Format(operatorStringFormat, formatArgs);
            }
            resultColumn.Name = operatorStringFormat;
            _entity.Columns.Add(resultColumn);
            AddTo(_selectSection, resultColumn.Name);

        }


        Sort _orderBy = new Sort();
        public Sort OrderBy
        {
            get { return _orderBy; }
            set { _orderBy = value; }
        }

        FilterCollection _where = new FilterCollection();
        public FilterCollection Where { get { return _where; } }
        FilterCollection _having = new FilterCollection();
        public FilterCollection Having { get { return _having; } }

    }
}