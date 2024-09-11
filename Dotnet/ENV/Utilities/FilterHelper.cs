using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENV.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;

namespace ENV.Utilities
{
    public static class FilterHelper
    {
      
        public static string ToSQLWhere(FilterBase where,bool useWildCards, params Firefly.Box.Data.Entity[] contextEntites)
        {
            if (where == null)
                return string.Empty;
            var f = FilterBase.GetIFilter(where, useWildCards, contextEntites);
            var isOracle=false;
            if (contextEntites.Length > 0)
            {
                var e = contextEntites[0] as ENV.Data.Entity;
                if (e != null)
                {
                    var d = e.DataProvider as DynamicSQLSupportingDataProvider;
                    if (d != null)
                    {
                        isOracle = d.IsOracle;
                    }
                }
            }
            return SQLFilterConsumer.DisplayFilterInSingleLine(f,isOracle);

        }

        public static string ToSQLWhere(AbstractUIController uic, FilterCollection where)
        {
            return ToSQLWhere(uic.From, uic.Relations, where,true);
        }
        public static string ToSQLWhere(BusinessProcessBase uic, FilterCollection where)
        {
            return ToSQLWhere(uic.From, uic.Relations, where, true);
        }
        public static string ToSQLWhere(AbstractUIController uic, Relation relation)
        {
            if (relation.Type == RelationType.Join || relation.Type == RelationType.OuterJoin)
                return ToSQLWhere(uic, relation.Where);
            return ToSQLWhere(relation.Where,false, relation.From);
        }
    
        public static string ToSQLWhere(Firefly.Box.Data.Entity from, RelationCollection relations, FilterCollection where,bool useWildCards)
        {
            var l = new List<Firefly.Box.Data.Entity>();
            if (from != null)
                l.Add(from);
            foreach (var r in relations)
            {
                if (r.Type == RelationType.Join || r.Type == RelationType.OuterJoin)
                    l.Add(r.From);
            }
            return ToSQLWhere(where,useWildCards, l.ToArray());
        }
    }
}
