using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ENV.Printing;

namespace ENV.IO.Advanced
{
    /// <summary>
    /// Used to annotate text in text printing.
    /// </summary>
    /// /// <remarks>
    /// When printing to a text printer, special string notation such as 'Bold' or 'Italic' are represented as character that are usually called escape codes.
    /// To support this behavior any <see cref="TextControlBase"/> has a <see cref="TextControlBase.TextPrintingStyle"/> property that determines the wished notation.
    /// Because the way 'Bold' is used in deferent printers, The to interpret the 'Bold' notation is determined at the <see cref="Printer"/>'s <see cref="Printing.Printer.AddTextPrintingToken"/> method.
    /// The key string used in the <see cref="Printer"/>'s <see cref="Printing.Printer.AddTextPrintingToken"/> is referenced by the printTags parameter to this constructor
    /// <demo/>
    /// </remarks>

    public class TextPrintingStyle : System.ComponentModel.IComponent
    {
        List<string> _printTags;


        public TextPrintingStyle(params string[] printTags)
        {
            _printTags = printTags.ToList();
        }

        static readonly TextPrintingStyle _default = new TextPrintingStyle();

        public event EventHandler Disposed;

        public static TextPrintingStyle Default { get { return _default; } }

        public ISite Site
        {
            get; set;
        }
        internal void SendPrefix(Printer printer, Action<string> prefixAction)
        {
            foreach (string printTag in _printTags)
            {
                printer.SetPrintTag(printTag,
                    delegate (string prefix, string suffix)
                    {
                        prefixAction(prefix);
                    });
            }
        }

        internal void SendSuffix(Printer printer, Action<string> suffixAction)
        {
            foreach (string printTag in _printTags)
            {
                printer.SetPrintTag(printTag,
                    delegate (string prefix, string suffix)
                    {
                        suffixAction(suffix);
                    });
            }
        }

        public override bool Equals(object obj)
        {
            TextPrintingStyle item = obj as TextPrintingStyle;
            if (item == null)
                return base.Equals(obj);
            if (_printTags.Count != item._printTags.Count)
                return false;
            for (int i = 0; i < _printTags.Count; i++)
            {
                if (_printTags[i] != item._printTags[i])
                    return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            return _printTags.GetHashCode();
        }

        public void Decorate(StringBuilder sb, Printer printer, string s)
        {
            Decorate(sb, printer, s, false, false);
        }

        public void Decorate(StringBuilder sb, Printer printer, string s, bool ignorePrefix, bool ignoreSuffix)
        {
            printer.Decorate(sb, _printTags, s, ignorePrefix, ignoreSuffix);
        }

        public void Dispose()
        {
            if (Disposed != null)
                Disposed(this, new EventArgs());
            

        }
    }
}
