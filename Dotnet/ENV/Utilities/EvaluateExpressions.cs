using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ENV.Security.Entities;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using Comparer = Firefly.Box.Advanced.Comparer;
using ENV.BackwardCompatible;
using ENV.Security;

namespace ENV.Utilities
{
    class EvaluateExpressions
    {

        class MyMethod
        {
            MethodInfo _method;
            object _instance;
            string _name;
            public MyMethod(MethodInfo m, object instance, string name)
            {
                _method = m;
                _instance = instance;
                _name = name;
            }
            public string GetName()
            {
                return _name;
            }

            internal ParameterInfo[] GetParameters()
            {
                return _method.GetParameters();
            }

            internal object Invoke(object[] args)
            {
                return _method.Invoke(_instance, args);
            }
        }
        Func<INullStrategy> _nullStrategy;
        Func<RolesCollection> _getRoles;
        public EvaluateExpressions(object objectToRunFunctionsOn, Func<Firefly.Box.Advanced.TaskCollection> activeTasks, Func<INullStrategy> nullStrategy = null, Func<RolesCollection> getRoles = null)
        {
            if (getRoles == null)
                getRoles = () => null;
            _getRoles = getRoles;
            _nullStrategy = nullStrategy;
            if (_nullStrategy == null)
                _nullStrategy = () => NullStrategy.GetStrategy(false);
            AddObjectForFunctions((x) => x(objectToRunFunctionsOn));
            _methods = delegate
            {
                var loaded = new HashSet<object>();
                var result = new SortedDictionary<string, List<MyMethod>>();
                foreach (var f in _objects)
                {
                    f(item =>
                    {

                        if (item == null)
                            return;
                        if (loaded.Contains(item))
                            return;
                        loaded.Add(item);
                        MyMethod @case = null;
                        foreach (var method in item.GetType().GetMethods())
                        {
                            if (method.Name.ToUpper() == "CASE")
                                continue;
                            List<MyMethod> methodsByname;
                            var name = method.Name;
                            if (name.EndsWith("_"))
                                name = name.Substring(0, name.Length - 1);
                            if (!result.TryGetValue(name.ToUpper(CultureInfo.InvariantCulture), out methodsByname))
                            {
                                result.Add(name.ToUpper(CultureInfo.InvariantCulture), methodsByname = new List<MyMethod>());
                            }
                            var m = new MyMethod(method, item, name);
                            methodsByname.Add(m);
                            if (name == "CaseUntyped")
                                @case = m;
                        }
                        if (@case != null)
                            result.Add("CASE", new List<MyMethod> { @case });
                    });

                }



                _methods = () => result;
                return result;
            };
            _identifiers = () =>
            {
                var result = CreateColumnIdentifierList(activeTasks());
                _identifiers = () => result;
                return result;
            };
        }

        internal static SortedDictionary<string, ColumnBase> CreateColumnIdentifierList(Firefly.Box.Advanced.TaskCollection activeTasks = null)
        {
            if (activeTasks == null)
                activeTasks = Context.Current.ActiveTasks;
            var result = new SortedDictionary<string, ColumnBase>();
            int i = 0;
            foreach (var task in activeTasks)
            {
                foreach (var column in ControllerBase.GetColumnsOf(task))
                {
                    i++;
                    result.Add(ConvertNumberToLetters(i), column);
                }
            }

            return result;
        }

        public void AddObjectForFunctions(Action<Action<object>> add)
        {
            _objects.Add(add);
        }

        public static string ConvertNumberToLetters(int i)
        {

            int a = 0, b = 0, c = 0;
            while (i > 26)
            {
                c++;
                i -= 26;
                if (c > 25)
                {
                    b++;
                    c -= 26;
                }
            }

            if (b > 0)
                return ConvertLetter(b + 1) + ConvertLetter(c + 1) + ConvertLetter(i);
            if (c > 0)
                return ConvertLetter(c + 1) + ConvertLetter(i);
            return ConvertLetter(i);

        }
        static string ConvertLetter(int i)
        {
            return ((char)((int)'A' + i - 1)).ToString();
        }
        public T Evaluate<T>(string expression)
        {
            var e = new ExpressionTree(new StringTokensProvider(expression));
            var t = new TreeVisitor(this);
            e.Result.Visit(t);
            return t.GetResult<T>();

        }
        public string GetExpression(string expression)
        {
            var e = new ExpressionTree(new StringTokensProvider(expression));
            var v = new ExpressionStringVisitor(this);
            e.Result.Visit(v);
            return v.Result;
        }
        public Text GetReturnValue(Text expression)
        {
            var e =
                new ExpressionTree(new StringTokensProvider(expression));
            var t = new TreeVisitor(this);
            e.Result.Visit(t);
            var x = t.GetRawResult();
            var r = TypeToLetter(x);
            if (r == "*")
            {
                if (x == null)
                {
                    var gt = new GetTypeTreeVisitor(this);
                    e.Result.Visit(gt);
                    if (gt.Result != null)
                        return gt.Result;
                }
            }
            return r;
        }
        string TypeToLetter(object x)
        {
            if (x is Number)
                return "N";
            if (x is Text)
                return "A";
            if (x is Bool)
                return "L";
            return "*";
        }
        class GetTypeTreeVisitor : NodeVisitor
        {
            EvaluateExpressions _parent;
            public Text Result = null;
            public GetTypeTreeVisitor(EvaluateExpressions parent)
            {
                _parent = parent;
            }

            public void Decimal(decimal value)
            {
            }

            public void Function(string name, ExpressionNode[] parameters)
            {
            }

            public void Identifier(string name)
            {
                ColumnBase c;
                if (_parent._identifiers().TryGetValue(name.ToUpperInvariant(), out c))
                {
                    Result = ENV.UserMethods.GetAttribute(c);
                }
            }

            public void Indexer(string name, ExpressionNode[] toArray)
            {

            }

            public void Operator(string name, ExpressionNode left, ExpressionNode right)
            {
                switch (name)
                {
                    case "+":
                    case "-":
                    case "*":
                    case "/":
                    case "^":
                    case "MOD":
                        Result = "N";
                        break;
                    case "&":
                        Result = "A";
                        break;
                    case "=":
                    case "<>":
                    case "<":
                    case "<=":
                    case ">":
                    case ">=":
                    case "AND":
                    case "OR":
                        Result = "L";
                        break;
                }
            }

            public void String(string value, string metaData)
            {
                Result = "A";
            }
        }
        public T Evaluate<T>(string expression, T defaultValue)
        {
            try
            {
                if (Text.IsNullOrEmpty(expression))
                    return defaultValue;
                return Evaluate<T>(expression);
            }
            catch
            {
                return defaultValue;
            }


        }
        class ExpressionStringVisitor : NodeVisitor
        {
            private EvaluateExpressions _parent;

            public string Result = null;

            public bool IsArray
            {
                get { return Result.GetType().IsArray; }
            }

            public ExpressionStringVisitor(EvaluateExpressions _parent)
            {
                this._parent = _parent;
            }

            public void Decimal(decimal value)
            {
                Result = value.ToString();
            }

            public void String(string value, string metaData)
            {
                Result = "'" + value.Replace("'", "''") + "'";
                char c = ' ';
                if (metaData.Length > 0)
                    c = metaData[0].ToString().ToUpper(CultureInfo.InvariantCulture)[0];
                switch (c)
                {

                    case 'L':
                        Result += "LOG";
                        break;
                    case 'D':
                        Result += "DATE";
                        break;
                    case 'T':
                        Result += "TIME";
                        break;
                    case 'P':
                    case 'F':
                        Result += "FILE";
                        break;

                    case 'K':
                        Result += "KEY";
                        break;
                    case 'R':
                        Result += "RIGHT";
                        break;
                    default:
                        break;

                }
            }
            string _operator;

            public void Operator(string name, ExpressionNode leftExpression, ExpressionNode rightExpression)
            {
                name = name.ToUpper();
                _operator = name;
                ExpressionStringVisitor left = new ExpressionStringVisitor(_parent),
                            right = new ExpressionStringVisitor(_parent);
                leftExpression.Visit(left);
                rightExpression.Visit(right);
                left.YouAreAChildOfBinary(name, true);
                right.YouAreAChildOfBinary(name, false);

                Result = left.Result + " " + Result + name + " " + right.Result;

            }
            void YouAreAChildOfBinary(string parentOp, bool leftSide)
            {
                if (_operator == null)
                    return;
                switch (parentOp)
                {
                    case "-":
                        switch (_operator)
                        {
                            case "*":
                            case "/":
                            case "%":
                            case "^":
                                return;
                            case "+":
                            case "-":
                                if (leftSide)
                                    return;
                                break;
                        }
                        break;
                    case "+":
                        switch (_operator)
                        {
                            case "+":



                                return;
                            case "*":
                            case "/":
                            case "%":
                            case "^":
                                return;
                        }
                        if (leftSide && _operator == "-")
                            return;
                        break;
                    case "/":
                        switch (_operator)
                        {
                            case "^":
                                return;
                            default:
                                if (leftSide)
                                {
                                    switch (_operator)
                                    {
                                        case "*":
                                        case "%":
                                        case "/":
                                            return;
                                    }
                                }
                                break;
                        }
                        break;
                    case "*":
                        switch (_operator)
                        {
                            case "^":
                            case "*":
                                return;
                            default:
                                if (leftSide)
                                {
                                    switch (_operator)
                                    {
                                        case "%":
                                        case "*":
                                            return;
                                    }
                                }
                                break;
                        }
                        break;

                    case "%":
                        switch (_operator)
                        {
                            case "^":
                            case "*":
                            case "/":
                                if (leftSide)
                                    return;
                                break;
                        }
                        break;

                    case "&&":
                    case "&":
                    case "u.And":
                    case "AND":
                        switch (_operator)
                        {
                            case "||":
                            case "|":
                            case "u.Or":
                                break;
                            default:
                                return;
                        }
                        break;

                    case "||":
                    case "|":
                    case "u.Or":
                    case "OR":
                        return;
                    case "==":
                    case "!=":
                    case "<":
                    case ">":
                    case ">=":
                    case "<=":
                        {
                            bool noSograim = true;
                            switch (_operator)
                            {
                                case "==":
                                case "!=":
                                case "<":
                                case ">":
                                case ">=":
                                case "<=":
                                case "||":
                                case "&&":
                                    noSograim = false;
                                    break;
                            }
                            if (noSograim)
                                return;
                        }
                        break;

                }

                Result = "(" + Result + ")";
            }

            public void Function(string name, ExpressionNode[] parameters)
            {

                var args = new List<ExpressionStringVisitor>();
                foreach (var node in parameters)
                {
                    var tv = new ExpressionStringVisitor(_parent);
                    node.Visit(tv);
                    args.Add(tv);
                }
                if (name == "-" && args.Count == 1)
                {
                    Result = "-" + args[0].Result;
                    return;
                }

                List<MyMethod> methodsByName;
                if (!_parent._methods().TryGetValue(name.ToUpper(CultureInfo.InvariantCulture), out methodsByName))
                    throw new ExpressionParseException("Unknown function " + name);
                Result = " (";
                foreach (var p in args)
                {
                    if (Result.Length > 2)
                        Result += ", ";
                    Result += p.Result;
                }
                var method = methodsByName[0].GetName();
                if (method.Equals("NOT", StringComparison.InvariantCultureIgnoreCase))
                    method = "NOT";
                Result = method + Result + ")";


            }

            public void Identifier(string name)
            {
                ColumnBase x;
                if (_parent._identifiers().TryGetValue(name.ToUpper(CultureInfo.InvariantCulture), out x))
                {
                    Result = name.ToUpper();
                }
                else
                    throw new ExpressionParseException("Unknown identifier - " + name);
            }

            public void Indexer(string name, ExpressionNode[] toArray)
            {
                throw new NotImplementedException();
            }

        }

        class TreeVisitor : NodeVisitor
        {
            private EvaluateExpressions _parent;

            private object _result = null;
            bool _canBeKeys = false;
            public bool IsArray
            {
                get { return _result.GetType().IsArray; }
            }

            public TreeVisitor(EvaluateExpressions _parent)
            {
                this._parent = _parent;
            }

            public void Decimal(decimal value)
            {
                _result = Number.Create(value);
            }

            public void String(string value, string metaData)
            {
                char c = ' ';
                if (metaData.Length > 0)
                    c = metaData[0].ToString().ToUpper(CultureInfo.InvariantCulture)[0];
                switch (c)
                {

                    case 'L':
                        _result = (Bool)value.ToUpper(CultureInfo.InvariantCulture).StartsWith("T");
                        break;
                    case 'D':
                        if (metaData.ToUpperInvariant().StartsWith("DSOURCE"))
                            _result = Number.Parse(value);
                        else _result = Date.Parse(value, "");
                        break;
                    case 'T':
                        _result = Time.Parse(value, "");
                        break;
                    case 'P':
                    case 'F':
                        _result = Number.Parse(value);
                        break;

                    case 'K':
                        _result = (Text)value;
                        _canBeKeys = true;
                        break;
                    case 'R':
                        _result = Number.Zero; ;
                        if (!Text.IsNullOrEmpty(value))
                        {
                            var x = _parent._getRoles();
                            if (x != null)
                            {
                                var y = x.GetByName(value);
                                if (y == 0 && value.Contains("."))
                                    y = UserMethods.keyOfRightOtherApplication;
                                _result = y;
                            }
                        }
                        break;
                    default:
                        _result = (Text)value;
                        break;

                }
            }

            public void Operator(string name, ExpressionNode leftExpression, ExpressionNode rightExpression)
            {
                TreeVisitor left = new TreeVisitor(_parent),
                            right = new TreeVisitor(_parent);
                leftExpression.Visit(left);
                rightExpression.Visit(right);
                Func<Func<Number, Number, Number>, Number> numericOperator
                    = delegate (Func<Number, Number, Number> func)
                    {
                        return func(left.GetNumber(), right.GetNumber());
                    };
                Func<Func<int, Bool>, Bool> compare
                    = delegate (Func<int, Bool> compareResult)
                    {
                        Number a, b;
                        if (left.TryGet<Number>(out a) && right.TryGet<Number>(out b))
                            return compareResult(Comparer.Compare(a, b));
                        return compareResult(Comparer.Compare(left._result, right._result, true));


                    };
                Func<Bool> equal
                  = delegate ()
                  {
                      Number a, b;
                      if (left.TryGet<Number>(out a) && right.TryGet<Number>(out b))
                          return _parent._nullStrategy().Equals(a, b);
                      var l = _parent.TypeToLetter(left._result);
                      var r = _parent.TypeToLetter(right._result);
                      if (l != "*" && r != "*" && l != r)
                          throw new ExpressionParseException("Invalid Expression");
                      return _parent._nullStrategy().Equals(left._result, right._result);


                  };
                switch (name.ToUpper(CultureInfo.InvariantCulture))
                {
                    case "+":
                        _result = numericOperator((a, b) => a + b);
                        break;
                    case "-":
                        _result = numericOperator((a, b) => a - b);
                        break;
                    case "*":
                        _result = numericOperator((a, b) => a * b);
                        break;
                    case "/":
                        _result = numericOperator((a, b) => a / b);
                        break;
                    case "^":
                        _result = numericOperator(ENV.UserMethods.Instance.Pow);
                        break;
                    case "MOD":
                        _result = numericOperator((a, b) => a % b);
                        break;
                    case "&":
                        _result = left.GetText() + right.GetText();
                        break;
                    case "=":
                        _result = equal();
                        break;
                    case "<>":
                        _result = !equal();
                        break;
                    case "<":
                        _result = compare(x => x < 0);
                        break;
                    case "<=":
                        _result = compare(x => x <= 0);
                        break;
                    case ">":
                        _result = compare(x => x > 0);
                        break;
                    case ">=":
                        _result = compare(x => x >= 0);
                        break;
                    case "AND":
                        _result = (Bool)(left.GetResult<Bool>() && right.GetResult<Bool>());
                        break;
                    case "OR":
                        _result = (Bool)(left.GetResult<Bool>() || right.GetResult<Bool>());
                        break;

                    default:
                        throw new ExpressionParseException("Unknown uperator " + name);
                }
            }

            public void Function(string name, ExpressionNode[] parameters)
            {
                if (name.ToUpper() == "IF")
                {
                    var tv = new TreeVisitor(_parent);
                    parameters[0].Visit(tv);
                    var r = tv.GetResult<Bool>() ? parameters[1] : parameters[2];
                    tv = new TreeVisitor(_parent);
                    r.Visit(tv);
                    _result = tv._result;
                }
                else
                {
                    var args = new List<TreeVisitor>();
                    foreach (var node in parameters)
                    {
                        var tv = new TreeVisitor(_parent);
                        node.Visit(tv);
                        args.Add(tv);
                    }

                    _result = _parent.EvaluateFunction(name, args);
                }


            }

            public void Identifier(string name)
            {
                ColumnBase x;
                if (_parent._identifiers().TryGetValue(name.ToUpper(CultureInfo.InvariantCulture), out x))
                {
                    _result = x.Value;
                }
                else
                    throw new ExpressionParseException("Unknown identifier - " + name);
            }

            public void Indexer(string name, ExpressionNode[] toArray)
            {
                throw new NotImplementedException();
            }

            public T GetResult<T>()
            {
                T result;
                if (TryGet<T>(out result))
                    return result;
                throw Throw(typeof(T));

            }
            public bool TryGet<T>(out T to)
            {
                bool ok = false;
                T result = default(T);
                TryGet(typeof(T), delegate (object value)
                {
                    ok = true;
                    result = (T)value;
                });
                to = result;
                return ok;
            }
            public bool TryGet(Type T, out object to)
            {
                bool ok = false;
                object result = null;
                TryGet(T, delegate (object value)
                {
                    ok = true;
                    result = value;
                });
                to = result;
                return ok;
            }

            public void TryGet(Type T, Action<object> to)
            {
                if (_result == null)
                    to(null);
                else if (T.IsAssignableFrom(_result.GetType()))
                    to(_result);
                else if (typeof(Number).IsAssignableFrom(T))
                    TryGetNumber(a => to(a));
                else if (typeof(Text).IsAssignableFrom(T))
                    TryGetText(a => to(a));
                else if (typeof(Date).IsAssignableFrom(T))
                    to(ENV.UserMethods.Instance.ToDate(GetNumber()));
                else if (typeof(Time).IsAssignableFrom(T))
                    to(ENV.UserMethods.Instance.ToTime(GetNumber()));
                else if (T.IsAssignableFrom(typeof(Keys)) && _canBeKeys)
                    to(KBPutParserClient.FindKey(_result.ToString()));
            }

            internal Number GetNumber()
            {
                Number result = null;
                bool ok = false;
                TryGetNumber(delegate (Number number)
                {
                    result = number;
                    ok = true;
                });
                if (ok)
                    return result;
                throw Throw(typeof(Number));
            }
            void TryGetNumber(Action<Number> to)
            {
                Type t = _result.GetType();
                if (typeof(Number).IsAssignableFrom(t))
                    to((Number)_result);
                else if (typeof(Date).IsAssignableFrom(t))
                    to(ENV.UserMethods.Instance.ToNumber((Date)_result));
                else if (typeof(Time).IsAssignableFrom(t))
                    to(ENV.UserMethods.Instance.ToNumber((Time)_result));
                else
                {
                    Number result;
                    if (Number.TryCast(_result, out result))
                        to(result);

                }
            }

            Exception Throw(Type expectedType)
            {
                return new ExpressionParseException(string.Format("Cant convert {0} of type {1} to {2}", _result, _result != null ? _result.GetType().Name : "null", expectedType.Name,
                                                                   typeof(Number).Name));
            }

            Text GetText()
            {
                Text result = null;
                bool ok = false;
                TryGetText(delegate (Text text)
                {
                    result = text;
                    ok = true;
                });
                if (ok)
                    return result;
                throw Throw(typeof(Text));
            }
            void TryGetText(Action<Text> to)
            {
                Type t = _result.GetType();
                if (typeof(Text).IsAssignableFrom(t))
                    to((Text)_result);

            }


            public object GetRawResult()
            {
                return _result;
            }
        }


        Func<SortedDictionary<string, List<MyMethod>>> _methods;
        Func<SortedDictionary<string, ColumnBase>> _identifiers;
        List<Action<Action<object>>> _objects = new List<Action<Action<object>>>();
        object EvaluateFunction(string name, List<TreeVisitor> args)
        {
            if (name == "-" && args.Count == 1)
                return -1 * args[0].GetNumber();
            List<MyMethod> methodsByName;
            if (!_methods().TryGetValue(name.ToUpper(CultureInfo.InvariantCulture), out methodsByName))
                throw new ExpressionParseException("Unknown function " + name);
            foreach (var method in methodsByName)
            {
                try
                {
                    var parameters = new List<object>();
                    int i = 0;
                    var pars = method.GetParameters();
                    foreach (var p in pars)
                    {
                        if (i == pars.Length - 1 && p.ParameterType.IsArray && (args.Count >= i || !args[i].IsArray) && p.IsDefined(typeof(ParamArrayAttribute), false))
                        {
                            ArrayList paramsField = new ArrayList();
                            Type t = p.ParameterType.GetElementType();
                            while (i < args.Count)
                            {
                                object o = null;
                                if (args[i].TryGet(t, out o))
                                {
                                    paramsField.Add(o);
                                    i++;
                                }
                                else
                                    break;
                            }
                            parameters.Add(paramsField.ToArray(t));
                            break;
                        }
                        else
                        {
                            object o = null;
                            if (args[i].TryGet(p.ParameterType, out o))
                            {
                                parameters.Add(o);
                                i++;
                            }
                            else
                                break;
                        }
                    }
                    var finalParameters = parameters.ToArray();
                    if (i == args.Count)
                    {
                        switch (name.ToUpper(CultureInfo.InvariantCulture))
                        {
                            case "UDF":
                            case "UDFS":
                            case "UDFF":
                            case "CALLDLL":
                            case "CALLDLLF":
                            case "CALLDLLS":
                                finalParameters = UserMethods.UseValuesInsteadOfColumns(finalParameters);
                                break;

                        }
                        return method.Invoke(finalParameters);
                    }
                }
                catch { }

            }
            throw new ExpressionParseException("No overload for " + name + " was found that matches the parameters received");
        }

        public void ProvideMethodsTo(Action<string> to)
        {
            foreach (var m in _methods())
            {
                to(m.Key);
            }
        }

        public void ProvideColumnsTo(Action<string, ColumnBase> to)
        {
            foreach (var i in _identifiers())
            {
                to(i.Key, i.Value);
            }
        }


    }
}