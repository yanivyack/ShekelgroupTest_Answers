using ENV.Data;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ENV.Utilities
{
    class ExpressionBasedSQLFilter : ICustomFilterMember
    {
        string _expression;

        Func<SortedDictionary<string, ColumnBase>> _identifiers;
        static Dictionary<string, System.Reflection.MethodInfo> _methods = new Dictionary<string, System.Reflection.MethodInfo>();
        UserDbMethods.UnTypedUserDbMethods _db;
        public ExpressionBasedSQLFilter(string expression, UserDbMethods db, Func<SortedDictionary<string, ColumnBase>> identifiers = null)
        {
            _db = db._db;
            if (_methods.Count == 0)
            {
                lock (_methods)
                {
                    if (_methods.Count == 0)
                    {
                        foreach (var item in _db.GetType().GetMethods())
                        {
                            if (!item.Name.Equals("TODATETIME", StringComparison.InvariantCultureIgnoreCase))
                                _methods.Add(item.Name.ToUpper(), item);
                        }
                    }
                }
            }
            _expression = expression;
            _identifiers = identifiers;
            if (_identifiers == null)
                _identifiers = () =>
                {
                    var x = EvaluateExpressions.CreateColumnIdentifierList();
                    _identifiers = () => x;
                    return x;
                };
        }

        public void SendFilterTo(CustomFilterCollector sendFilterString)
        {
            var et = new ExpressionTree(new StringTokensProvider(_expression));
            var mnv = new myNodeVisitor(this);
            et.Result.Visit(mnv);
            sendFilterString("{0}", mnv.Result);

        }
        internal bool isValidForUseAsSqlExpression(AbstractUIController uic)
        {
            var x = new validColumnNodeVisitor(uic, this);
            var et = new ExpressionTree(new StringTokensProvider(_expression));
            
            et.Result.Visit(x);
            return x.Ok;


        }
        class validColumnNodeVisitor : NodeVisitor
        {
            HashSet<ColumnBase> _invalidColumns = new HashSet<ColumnBase>();
            ExpressionBasedSQLFilter _parent;
            public validColumnNodeVisitor(AbstractUIController uic, ExpressionBasedSQLFilter parent)
            {
                _parent = parent;
                var validEntities = new HashSet<Firefly.Box.Data.Entity>();
                if (uic.From != null)
                    validEntities.Add(uic.From);
                foreach (var item in uic.Relations)
                {
                    if (item.Type == RelationType.Join || item.Type == RelationType.OuterJoin)
                        validEntities.Add(item.From);

                }

                foreach (var item in uic.Columns)
                {
                    if (item.Entity == null || !validEntities.Contains(item.Entity))
                        _invalidColumns.Add(item);

                }
            }
            public void Decimal(decimal value)
            {
            }

            public void Function(string name, ExpressionNode[] parameters)
            {
                foreach (var item in parameters)
                {
                    item.Visit(this);
                }
            }
            public bool Ok = true;

            public void Identifier(string name)
            {
                var r = _parent._identifiers()[name.ToUpper()];
                if (_invalidColumns.Contains(r))
                    Ok = false;
            }

            public void Indexer(string name, ExpressionNode[] toArray)
            {
                foreach (var item in toArray)
                {
                    item.Visit(this);
                }
            }

            public void Operator(string name, ExpressionNode left, ExpressionNode right)
            {
                foreach (var item in new[] { left, right })
                {
                    item.Visit(this);
                }
            }

            public void String(string value, string metaData)
            {
            }
        }
        class myNodeVisitor : NodeVisitor
        {
            ExpressionBasedSQLFilter _parent;
            public myNodeVisitor(ExpressionBasedSQLFilter parent)
            {
                _parent = parent;
            }
            public void Decimal(decimal value)
            {
                Result = value;
            }

            public void Function(string name, ExpressionNode[] parameters)
            {
                if (name.Equals("null", StringComparison.InvariantCultureIgnoreCase))
                {
                    Result = new MyNull();
                    return;
                }
                if (name == "-")
                {
                    var mnv = new myNodeVisitor(_parent);
                    parameters[0].Visit(mnv);
                    Result = new Minus(mnv.Result);
                    return;
                }
                var p = new List<object>();
                foreach (var item in parameters)
                {
                    var mnv = new myNodeVisitor(_parent);
                    item.Visit(mnv);
                    p.Add(mnv.Result);
                }
                if (name.Equals("IN", StringComparison.InvariantCultureIgnoreCase))
                {
                    var r = p;
                    p = new List<object>();
                    p.Add(r[0]);
                    r.RemoveAt(0);
                    p.Add(r.ToArray());
                }
                if (name.Equals("VAL", StringComparison.InvariantCultureIgnoreCase))
                {
                    p.RemoveAt(p.Count - 1);
                }
                Result = _methods[name.ToUpper()].Invoke(_parent._db, p.ToArray());
            }
            class MyNull : ICustomFilterMember
            {
                public void SendFilterTo(CustomFilterCollector sendFilterString)
                {
                    sendFilterString("null");
                }
            }
            class Minus : ICustomFilterMember
            {
                object _what;
                public Minus(object what)
                {
                    _what = what;
                }
                public void SendFilterTo(CustomFilterCollector sendFilterString)
                {
                    sendFilterString("-({0})", _what);
                }
            }

            public void Identifier(string name)
            {
                Result = _parent._identifiers()[name.ToUpper()];
            }

            public void Indexer(string name, ExpressionNode[] toArray)
            {
                throw new NotImplementedException();
            }

            public void Operator(string name, ExpressionNode left, ExpressionNode right)
            {
                var l = new myNodeVisitor(_parent);
                var r = new myNodeVisitor(_parent);
                left.Visit(l);
                right.Visit(r);
                if (name.Equals("MOD", StringComparison.InvariantCultureIgnoreCase))
                    name = "%";
                if (name == "&")
                    name = "+";
                if (l.Result is MyNull)
                {
                    if (name == "=" || name == "<>")
                    {
                        var x = l;
                        l = r;
                        r = x;
                    }
                }
                if (r.Result is MyNull)
                {
                    if (name == "=")
                    {
                        Result = _parent._db.SQL("({0} is null)", l.Result);
                        return;
                    }
                    if (name == "<>")
                    {
                        Result = _parent._db.SQL("({0} is not null)", l.Result);
                        return;
                    }
                }

                if (name == "=" && r.Result is MyNull)
                {

                }
                else
                    Result = _parent._db.SQL("({0} " + name + " {1})", l.Result, r.Result);
            }

            public void String(string value, string metaData)
            {
                if (metaData.StartsWith("Date", StringComparison.InvariantCultureIgnoreCase))
                {
                    Result = _parent._db.ToDateTime(Date.Parse(value, ""));
                    return;
                }
                if (metaData.StartsWith("LOG", StringComparison.InvariantCultureIgnoreCase))
                {
                    Result = value.StartsWith("T", StringComparison.InvariantCultureIgnoreCase) ? 1 : 0;
                    return;
                }


                Result = value;
            }
            public object Result = "Invalid sql expression";
        }


    }
}
