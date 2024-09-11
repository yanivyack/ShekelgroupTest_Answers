using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ENV.IO.Advanced;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;

namespace ENV.IO
{
    public class TextTemplate : Advanced.TextTemplate
    {
        public TextTemplate(TextReader reader)
            : base(reader)
        {
            init();
        }


        public TextTemplate(string templateFile)
            : base(templateFile)
        {
            init();
        }

        void init()
        {
            TokenPrefix = "<!$";
            TokenSuffix = ">";
        }
        public override string TokenPrefix
        {
            get { return base.TokenPrefix; }
            set { base.TokenPrefix = value + "MG"; }
        }
    }
    public class Tag
    {
        Func<string> _tagExpression;
        Func<object> _valueExpression;
        Func<string> _formatExpression;

        public Tag(Func<string> tagExpression, Func<object> valueExpression, Func<string> formatExpression)
        {
            _tagExpression = tagExpression;
            _valueExpression = valueExpression;
            _formatExpression = formatExpression;
        }
        public Tag(string tag, Func<object> valueExpression)
            : this(delegate { return tag; }, valueExpression)
        {

        }
        public Tag(Func<string> tagExpression, Func<object> value)
            : this(tagExpression, value, (Func<string>)null)
        {

        }
        public Tag(Func<string> tagExpression, Func<object> value, string format)
            : this(tagExpression, value, () => format)
        {

        }
        public Tag(Func<string> tagExpression, object value, Func<string> format)
            : this(tagExpression, () => value, format)
        {

        }
        public Tag(Func<string> tagExpression, object value, string format)
            : this(tagExpression, () => value, () => format)
        {

        }
        public Tag(Func<string> tagExpression, object value)
            : this(tagExpression, delegate { return value; }, (Func<string>)null)
        {

        }
        public Tag(string tag, object value)
            : this(delegate { return tag; }, delegate { return value; }, (Func<string>)null)
        {

        }

        public Tag(string tag, object value, string format)
            : this(delegate { return tag; }, delegate { return value; }, () => format)
        {

        }
        public Tag(string tag, object value, Func<string> format)
            : this(delegate { return tag; }, delegate { return value; }, format)
        {

        }
        public Tag(string tag, Func<object> value, string format)
            : this(delegate { return tag; }, value, () => format)
        {

        }
        public Tag(string tag, Func<object> value, Func<string> format)
            : this(delegate { return tag; }, value, format)
        {

        }



        internal virtual void ApplyTo(TemplateValues templateValues, bool replaceXmlSpecialCharacters)
        {
            object value;
            if (_valueExpression != null)//W1872
                value = _valueExpression();
            else
                value = "";
            var tagName = (_tagExpression() ?? "").TrimEnd();
            Bool bVal;
            if (Bool.TryCast(value, out bVal))
                templateValues.Add(tagName, bVal);
            string val;
            var c = value as ColumnBase;
            if (c != null && c.Value == null)
            {
                val = c.NullDisplayText;
            }
            else
            {
                
                if (_formatExpression == null)
                {
                    var tc = value as ENV.Data.TextColumn;
                    if (tc != null)
                    {
                        val = tc.ToString().TrimEnd();
                    }
                    else
                      val = value.ToString();
                }
                else
                {
                    val = ENV.UserMethods.Instance.ToString(value, _formatExpression());
                    if (val == "")
                        val = string.Empty;
                }
            }
            
            if (replaceXmlSpecialCharacters)
                val = ENV.UserMethods.Instance.XMLVal(val);
            if(val != null)
                val = val.TrimEnd('\0');
            templateValues.Add(tagName, val);
        }
    }
}
