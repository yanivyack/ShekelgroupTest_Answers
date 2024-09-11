using ENV.IO.Advanced;
using ENV.IO.Advanced.Internal;
using System.Drawing;

namespace ENV.IO
{
    [ToolboxBitmap(typeof(System.Windows.Forms.Label))]
    public class TextLabel : TextControlBase, TextControl
    {
        

        protected override string Translate(string term)
        {
            return base.Translate(ENV.Languages.Translate(term));
        }
        internal override void SetYourValue(string value,IStringToByteArrayConverter c)
        {

        }

        internal override void WriteTo(Write writer)
        {
            writer(Translate(Text), true);
        }
        
    }
}
