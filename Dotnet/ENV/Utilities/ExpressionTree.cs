using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Firefly.Box;

namespace ENV.Utilities
{
    interface NodeVisitor
    {
        void Decimal(decimal value);
        void String(string value, string metaData);
        void Operator(string name, ExpressionNode left, ExpressionNode right);
        void Function(string name, ExpressionNode[] parameters);
        void Identifier(string name);
        void Indexer(string name, ExpressionNode[] toArray);
    }

    interface ExpressionNode
    {
        void Visit(NodeVisitor visitor);
    }

    class ExpressionTree : TokensConsumer
    {
        public ExpressionTree(TokensProvider provider)
        {
            _state = new StartOfExpression(this);
            provider.ProvideTokens(this);
            _state.Finish();

        }
        ExpressionNode _resultNode;
        public ExpressionNode Result { get { return _resultNode; } }
        class DecimalNode : ExpressionNode
        {
            decimal _value;

            public DecimalNode(decimal value)
            {
                _value = value;
            }

            public void Visit(NodeVisitor visitor)
            {
                visitor.Decimal(_value);
            }
        }
        private void SendNumber(string secondPart)
        {
            DecimalNode dn = null;
            var c = Number.DecimalSeparator.Value;
            if (c != '.')
            {
                Number.DecimalSeparator.SetContextValue('.');
            }
            try
            {


                dn = new DecimalNode(Number.Parse(secondPart));
            }
            finally
            {
                if (c != '.')
                    Number.DecimalSeparator.SetContextValue(c);
            }
            _state.Value(dn);
        }
        ConsumerState ____state;
        ConsumerState _state { get { return ____state; } set { ____state = value; StringParser.Debug(value, "TokenConsumer="); } }

        class ConsumerState
        {
            bool canHaveOr(ConsumerState parentConsumer)
            {
                return !(parentConsumer is NotState) && !(parentConsumer is FunctionState.OpenParenseOfFunction)
                   && !(parentConsumer is StartOfExpression) && !(parentConsumer is OperatorState);
            }
            protected bool RepeatOperationBecauseItHadAndOrNot(string myName, ConsumerState parentConsumer)
            {
                if (myName.StartsWith("NOT", StringComparison.InvariantCultureIgnoreCase) && myName.Length > 3)
                {
                    parentConsumer.Identifier("NOT");
                    _parent._state.Identifier(myName.Substring(3));
                    return true;
                }
                else if (myName.StartsWith("OR", StringComparison.InvariantCultureIgnoreCase) && myName.Length > 2 && canHaveOr(parentConsumer))
                {
                    parentConsumer.Identifier("OR");
                    _parent._state.Identifier(myName.Substring(2));
                    return true;
                }
                else if (myName.StartsWith("AND", StringComparison.InvariantCultureIgnoreCase) && myName.Length > 3 && canHaveOr(parentConsumer))
                {
                    parentConsumer.Identifier("AND");
                    _parent._state.Identifier(myName.Substring(3));
                    return true;
                }
                else if (myName.StartsWith("MOD", StringComparison.InvariantCultureIgnoreCase) && myName.Length > 3 && canHaveOr(parentConsumer))
                {
                    parentConsumer.Identifier("MOD");
                    SendNumOrIdentifier(myName.Substring(3));
                    return true;
                }
                else
                    return false;
            }

            private void SendNumOrIdentifier(string secondPart)
            {
                if (char.IsDigit(secondPart[0]))
                {
                    _parent.SendNumber(secondPart);
                }
                else
                    _parent._state.Identifier(secondPart);
            }



            protected ExpressionTree _parent;

            public ConsumerState(ExpressionTree parent)
            {
                _parent = parent;
            }

            public virtual void Value(ExpressionNode value)
            {
                throw new ExpressionParseException("Value not supposed to be here");
            }

            public virtual void Identifier(string name)
            {
                if (name.ToUpper(CultureInfo.InvariantCulture) == "NOT" || name == "-")
                {
                    _parent._state = new NotState(_parent, name, this);
                }
                else
                    _parent._state = new FunctionState(_parent, name, this);
            }

            public virtual void Separator(char c)
            {
                throw new ExpressionParseException("Didn't anticipate separator " + c);
            }

            public virtual void Finish()
            {
                throw new ExpressionParseException("Missing end of expression");
            }
        }
        class StartOfExpression : ConsumerState
        {
            public StartOfExpression(ExpressionTree parent)
                : base(parent)
            {
            }
            public override void Value(ExpressionNode value)
            {
                _parent._state = new ValueAtStartOfExpression(_parent, this, value);
            }

            public override void Separator(char c)
            {
                if (c == '(')
                    _parent._state = new OpenParenseState(_parent, this);
                else
                    base.Separator(c);
            }
        }
        class NotState : ConsumerState
        {
            string _name;
            ConsumerState _parentConsumer;


            public NotState(ExpressionTree parent, string name, ConsumerState parentConsumer)
                : base(parent)
            {
                _name = name;
                _parentConsumer = parentConsumer;
            }

            public override void Separator(char c)
            {
                if (c == '(')
                {
                    (_parent._state = new FunctionState(_parent, _name, _parentConsumer)).Separator(c);
                }
            }
            public override void Value(ExpressionNode value)
            {
                _parent._state = new ValueAfterNotState(_parent, this, value);
            }
            class ValueAfterNotState : ConsumerState
            {
                ExpressionNode _value;
                NotState _parentConsumer;

                public ValueAfterNotState(ExpressionTree parent, NotState parentConsumer, ExpressionNode value)
                    : base(parent)
                {
                    _parentConsumer = parentConsumer;
                    _value = value;
                }
                public override void Identifier(string name)
                {
                    if ((_parentConsumer._name.ToUpperInvariant() == "NOT" && (
                        name.ToUpper(CultureInfo.InvariantCulture) == "OR" || name.ToUpper(CultureInfo.InvariantCulture) == "AND")) ||
                        _parentConsumer._name == "-")
                    {
                        TurnToFunction();
                        _parent._state.Identifier(name);
                    }
                    else
                    {
                        _parent._state = new OperatorState(_parent, _value, name, _parentConsumer);
                    }
                }
                public override void Separator(char c)
                {
                    if (c == ',' || c == ')' || c == ']')
                    {
                        TurnToFunction();
                        _parent._state.Separator(c);
                    }

                    else
                        base.Separator(c);
                }
                public override void Finish()
                {
                    TurnToFunction();
                    _parent._state.Finish();
                }

                void TurnToFunction()
                {
                    _parent._state = new FunctionState(_parent, _parentConsumer._name,
                                                       _parentConsumer._parentConsumer);
                    _parent._state.Value(_value);
                }
            }
        }
        class FunctionState : ConsumerState
        {
            string _name;
            ConsumerState _parentConsumer;
            List<ExpressionNode> _parameters = new List<ExpressionNode>();
            bool _squereBrakets = false;
            public FunctionState(ExpressionTree parent, string name, ConsumerState parentConsumer)
                : base(parent)
            {
                _name = name;
                _parentConsumer = parentConsumer;
            }
            public override void Separator(char c)
            {
                if (RepeatOperationBecauseItHadAndOrNot(_name, _parentConsumer))
                    _parent._state.Separator(c);
                else
                {
                    if (c == '(' || c == '[')
                    {
                        _parent._state = new OpenParenseOfFunction(_parent, this);
                        if (c == '[')
                            _squereBrakets = true;
                    }
                    else if (c == ')' || c == ',' || c == ']')
                    {
                        ChangeToIdentifier();
                        _parent._state.Separator(c);
                    }
                    else
                        base.Separator(c);
                }
            }
            public override void Value(ExpressionNode value)
            {
                _parameters.Add(value);
                GotAllParameters();
            }
            public override void Identifier(string name)
            {

                if (RepeatOperationBecauseItHadAndOrNot(_name, _parentConsumer))
                    _parent._state.Identifier(name);
                else
                    _parent._state = new IdentifierAfterFunction(_parent, name, this);
            }




            public override void Finish()
            {
                if (RepeatOperationBecauseItHadAndOrNot(_name, _parentConsumer))
                    _parent._state.Finish();
                else
                {
                    ChangeToIdentifier();
                    _parent._state.Finish();
                }
            }
            class IdentifierAfterFunction : ConsumerState
            {
                string _identifier;
                FunctionState _parentConsumer;

                public IdentifierAfterFunction(ExpressionTree parent, string identifier, FunctionState parentConsumer)
                    : base(parent)
                {
                    _identifier = identifier;
                    _parentConsumer = parentConsumer;
                }
                public override void Identifier(string name)
                {
                    if (RepeatOperationBecauseItHadAndOrNot(_identifier, _parentConsumer))
                        _parent._state.Identifier(name);
                    else
                    {
                        _parentConsumer.ChangeToIdentifier();
                        _parent._state.Identifier(_identifier);
                        _parent._state.Identifier(name);
                    }

                }
                public override void Value(ExpressionNode value)
                {
                    _parentConsumer.ChangeToIdentifier();
                    _parent._state.Identifier(_identifier);
                    _parent._state.Value(value);
                }
                public override void Separator(char c)
                {
                    if (RepeatOperationBecauseItHadAndOrNot(_identifier, _parentConsumer))
                        _parent._state.Separator(c);
                    else
                    {
                        if (c == '(')
                        {
                            _parentConsumer.ChangeToIdentifier();
                            _parent._state.Identifier(_identifier);
                            _parent._state.Separator(c);
                        }
                        if (c == ',')
                        {
                            _parentConsumer.Value(new IdentifierNode(_identifier));
                            _parentConsumer.GotAllParameters();
                            _parent._state.Separator(c);
                        }
                        if (c == ')' || c == ']')
                        {
                            _parentConsumer.Value(new IdentifierNode(_identifier));
                            _parentConsumer.GotAllParameters();
                            _parent._state.Separator(c);
                        }
                    }

                }
                public override void Finish()
                {
                    if (RepeatOperationBecauseItHadAndOrNot(_identifier, _parentConsumer))
                        _parent._state.Finish();
                    else
                    {
                        _parentConsumer.Value(new IdentifierNode(_identifier));
                        _parent._state.Finish();
                    }
                }
            }

            void ChangeToIdentifier()
            {
                if (_parameters.Count > 0)
                    throw new Exception();
                _parentConsumer.Value(new IdentifierNode(_name));
            }

            internal class OpenParenseOfFunction : ConsumerState
            {
                FunctionState _parentFunction;

                public OpenParenseOfFunction(ExpressionTree parent, FunctionState parentFunction)
                    : base(parent)
                {
                    _parentFunction = parentFunction;
                }
                public override void Value(ExpressionNode value)
                {
                    _parent._state = new ValueAfterOpenParenseOfFunction(_parent, this, value);
                }

                public void AddValue(ExpressionNode value)
                {
                    _parentFunction._parameters.Add(value);
                }
                public override void Separator(char c)
                {
                    switch (c)
                    {
                        case ')':
                        case ']':
                            _parentFunction.GotAllParameters();
                            break;
                        case '(':
                            _parent._state = new OpenParenseState(_parent, this);
                            break;
                        default:
                            base.Separator(c);
                            break;
                    }

                }
                class ValueAfterOpenParenseOfFunction : ConsumerState
                {
                    OpenParenseOfFunction _parentConsumerState;
                    ExpressionNode _value;

                    public ValueAfterOpenParenseOfFunction(ExpressionTree parent, OpenParenseOfFunction parentConsumerState, ExpressionNode value)
                        : base(parent)
                    {
                        _parentConsumerState = parentConsumerState;
                        _value = value;
                    }

                    public override void Identifier(string name)
                    {
                        _parent._state = new OperatorState(_parent, _value, name, _parentConsumerState);
                    }
                    public override void Separator(char c)
                    {
                        switch (c)
                        {
                            case ')':
                            case ']':
                                _parentConsumerState.AddValue(_value);
                                _parentConsumerState.Separator(c);
                                break;
                            case ',':
                                _parentConsumerState.AddValue(_value);
                                _parent._state = _parentConsumerState;
                                break;
                            case '[':
                                _parent._state = new OperatorState(_parent, _value, "[", _parentConsumerState);
                                _parent._state.Identifier("");
                                _parent._state.Separator(c);
                                break;
                            default:
                                base.Separator(c);
                                break;
                        }
                    }
                    public override void Finish()
                    {
                        Separator(')');
                        _parent._state.Finish();
                    }
                }
            }
            class FunctionNode : ExpressionNode
            {
                string _functionName;
                List<ExpressionNode> _parameters;
                bool _squereBrakets;

                public override string ToString()
                {
                    return _functionName + (_squereBrakets ? "[]" : "()");
                }
                public FunctionNode(string functionName, List<ExpressionNode> parameters, bool squereBrakets)
                {
                    _functionName = functionName;
                    _parameters = parameters;
                    _squereBrakets = squereBrakets;
                }

                public void Visit(NodeVisitor visitor)
                {
                    if (_squereBrakets)
                        visitor.Indexer(_functionName, _parameters.ToArray());
                    else
                        visitor.Function(_functionName, _parameters.ToArray());
                }
            }
            class IdentifierNode : ExpressionNode
            {
                string _name;

                public IdentifierNode(string name)
                {
                    _name = name;
                }

                public void Visit(NodeVisitor visitor)
                {
                    visitor.Identifier(_name);
                }
            }

            public void GotAllParameters()
            {
                _parentConsumer.Value(new FunctionNode(_name, _parameters, _squereBrakets));
            }
        }


        class ValueAtStartOfExpression : ConsumerState
        {
            ConsumerState _ParentConsumerState;
            ExpressionNode _value;

            public ValueAtStartOfExpression(ExpressionTree parent, ConsumerState parentConsumerState, ExpressionNode value)
                : base(parent)
            {
                _ParentConsumerState = parentConsumerState;
                _value = value;
            }
            public override void Identifier(string name)
            {
                if (RepeatOperationBecauseItHadAndOrNot(name, this)) { }
                else
                    _parent._state = new OperatorState(_parent, _value, name, _ParentConsumerState);
            }
            public override void Finish()
            {
                _parent._resultNode = _value;
            }
        }
        class OperatorState : ConsumerState
        {
            ConsumerState _ParentConsumerState;
            string _operator;
            ExpressionNode _leftSide;


            public OperatorState(ExpressionTree parent, ExpressionNode leftSide, string @operator, ConsumerState parentConsumerState)
                : base(parent)
            {
                _leftSide = leftSide;
                _operator = @operator;
                _ParentConsumerState = parentConsumerState;
            }

            public override void Value(ExpressionNode value)
            {
                _parent._state = new OperatorWithValueState(
                    _parent, _leftSide, _operator, _ParentConsumerState, this, value);
            }
            public override void Separator(char c)
            {
                switch (c)
                {
                    case '(':
                        _parent._state = new OpenParenseState(_parent, this);
                        break;
                    default:
                        base.Separator(c);
                        break;
                }
            }
        }
        class OpenParenseState : ConsumerState
        {
            ConsumerState _parentConsumer;

            public OpenParenseState(ExpressionTree parent, ConsumerState parentConsumer)
                : base(parent)
            {
                _parentConsumer = parentConsumer;
            }
            public override void Separator(char c)
            {
                if (c == '(')
                    _parent._state = new OpenParenseState(_parent, this);
                else
                    base.Separator(c);
            }

            public override void Value(ExpressionNode value)
            {
                _parent._state = new ValueAfterParense(_parent, _parentConsumer, this, value);
            }
        }
        class ValueAfterParense : ConsumerState
        {
            ConsumerState _ParentConsumerState;
            ConsumerState _openParense;
            ExpressionNode _value;

            public ValueAfterParense(ExpressionTree parent, ConsumerState parentConsumerState, ConsumerState openParense, ExpressionNode value)
                : base(parent)
            {
                _openParense = openParense;
                _ParentConsumerState = parentConsumerState;
                _value = value;
            }
            public override void Identifier(string name)
            {
                _parent._state = new OperatorState(_parent, _value, name, _openParense);
            }
            public override void Separator(char c)
            {
                switch (c)
                {
                    case ')':
                    case ']':
                        _parent._state = _ParentConsumerState;
                        _ParentConsumerState.Value(_value);
                        break;
                    default:
                        base.Separator(c);
                        break;
                }
            }
        }

        class OperatorWithValueState : ConsumerState
        {
            ConsumerState _parentConsumerState, _parentOperator;
            string _operator;
            ExpressionNode _leftSide, _rightSide;


            public OperatorWithValueState(ExpressionTree parent, ExpressionNode leftSide, string @operator, ConsumerState parentConsumerState, ConsumerState parentOperator, ExpressionNode rightSide)
                : base(parent)
            {
                _leftSide = leftSide;
                _operator = @operator;
                _parentConsumerState = parentConsumerState;
                _parentOperator = parentOperator;
                _rightSide = rightSide;
            }

            string _currentOperator;
            static string[] equalOperators = { "=", "<>", ">", "<", ">=", "<=", "NOT" };
            static string[] boolOperators = { "AND", "OR" };
            public override void Identifier(string name)
            {
                if (RepeatOperationBecauseItHadAndOrNot(name, this))
                { }
                else
                {
                    _currentOperator = name;
                    if (CurrentOperatorIs(".","[") && !PreviousOperatorWas(".", "[") ||
                        CurrentOperatorIs("^") && !PreviousOperatorWas("^") ||
                        PreviousOperatorWas("+", "-") && CurrentOperatorIs("*", "/", "MOD") ||
                        PreviousOperatorWas("OR") && CurrentOperatorIs("AND") ||
                        PreviousOperatorWas(boolOperators) && !CurrentOperatorIs(boolOperators) ||
                        PreviousOperatorWas(equalOperators) && !CurrentOperatorIs(equalOperators) && !CurrentOperatorIs(boolOperators))
                    {
                        _parent._state = new OperatorState(_parent, _rightSide, name, _parentOperator);

                    }
                    else
                    {
                        _parent._state = _parentConsumerState;
                        _parent._state.Value(new OperatorNode(_leftSide, _operator, _rightSide));
                        _parent._state.Identifier(name);
                    }
                }
            }
            bool PreviousOperatorWas(params string[] args)
            {
                string op = _operator.ToUpper(CultureInfo.InvariantCulture);
                foreach (var s in args)
                {
                    if (op == s)
                        return true;
                }
                return false;
            }
            bool CurrentOperatorIs(params string[] args)
            {
                string op = _currentOperator.ToUpper(CultureInfo.InvariantCulture);
                foreach (var s in args)
                {
                    if (op == s)
                        return true;
                }
                return false;
            }

            public override void Separator(char c)
            {
                if (c == ',' || c == ')' || c == ']')
                {
                    _parentConsumerState.Value(new OperatorNode(_leftSide, _operator, _rightSide));
                    _parent._state.Separator(c);
                }
                else
                    _parentConsumerState.Separator(c);
            }
            public override void Finish()
            {
                _parent._state = _parentConsumerState;
                _parent._state.Value(new OperatorNode(_leftSide, _operator, _rightSide));
                _parent._state.Finish();
            }
            class OperatorNode : ExpressionNode
            {
                string _operator;
                ExpressionNode _leftSide, _rightSide;

                public OperatorNode(ExpressionNode leftSide, string @operator, ExpressionNode rightSide)
                {
                    _leftSide = leftSide;
                    _operator = @operator;
                    _rightSide = rightSide;
                }

                public void Visit(NodeVisitor visitor)
                {
                    visitor.Operator(_operator, _leftSide, _rightSide);
                }
            }
        }


        public void Identifier(string name)
        {
            StringParser.Debug(name, "Identifier-");
            if (name.EndsWith("OR", StringComparison.InvariantCultureIgnoreCase) && name.Length > 2 && Char.IsDigit(name[0]))
            {
                SendNumber(name.Remove(name.Length - 2));
                _state.Identifier("OR");
            }
            else if (name.EndsWith("AND", StringComparison.InvariantCultureIgnoreCase) && name.Length > 3 && Char.IsDigit(name[0]))
            {
                SendNumber(name.Remove(name.Length - 3));
                _state.Identifier("AND");
            }
            else
                _state.Identifier(name);

        }

        public void DecimalValue(decimal value)
        {
            _state.Value(new DecimalNode(value));
        }

        public void Separator(char c)
        {
            StringParser.Debug(c, "Separator-");
            _state.Separator(c);
        }
        class StringNode : ExpressionNode
        {
            string _value;
            string _descriptor;

            public StringNode(string value, string descriptor)
            {
                _descriptor = descriptor;
                _value = value;
            }

            public void Visit(NodeVisitor visitor)
            {
                visitor.String(_value, _descriptor);
            }
        }
        public void StringValue(string value, string descriptor)
        {
            _state.Value(new StringNode(value, descriptor));
        }

    }
    public class ExpressionParseException : Exception
    {
        protected internal string _message;
        protected internal object[] _args;
        public ExpressionParseException(string message, params object[] args)
            : base(OptionalFormat(message, args))
        {
            _message = message;
            _args = args;
        }

        public ExpressionParseException(Exception innerException, string message, params object[] args)
            : base(OptionalFormat(message, args), innerException)
        {
            _message = message;
            _args = args;
        }


        static string OptionalFormat(string message, object[] args)
        {
            if (args == null || args.Length == 0)
                return message;
            return string.Format(message, args);
        }
    }
}