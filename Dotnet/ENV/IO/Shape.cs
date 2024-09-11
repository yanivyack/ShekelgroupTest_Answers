using ENV.IO.Advanced;
using System.Drawing;

namespace ENV.IO
{
    [ToolboxBitmap(typeof(Firefly.Box.UI.Shape))]
    public class Shape : TextControlBase
    {
        public Shape()
        {
            Multiline = true;
        }
        public bool Square { get; set; }
        public bool LineHorizontal { get; set; }
        public bool LineVertical { get; set; }

        protected override string Translate(string term)
        {
            return base.Translate(ENV.Languages.Translate(term));
        }
        internal override void WriteTo(Write writer)
        {
            var s = "";
            for (int i = 0; i < HeightInChars; i++)
            {
                var r = new string(' ', WidthInChars);
                if (Square)
                {
                    if (i == 0 || i == HeightInChars - 1)
                    {
                        r = "+" + new string('-', WidthInChars - 2) + "+";
                    }
                    else
                    {
                        r = "|" + new string(' ', WidthInChars - 2) + "|";
                    }
                }
                int mWidth = WidthInChars / 2;
                if (LineVertical)
                {
                    if (i > 0 && i < HeightInChars - 1)
                    {

                        r = r.Remove(mWidth, 1).Insert(mWidth, "|");

                    }
                }
                if (LineHorizontal)
                {
                    if (i == HeightInChars / 2)
                    {
                        r = new string('-', WidthInChars);
                        if (LineVertical)
                            r = r.Remove(mWidth, 1).Insert(mWidth, "+");
                    }
                }

                s += r;

            }
            writer(s, true);
        }

        internal override void SetYourValue(string value, IStringToByteArrayConverter c)
        {
        }

        internal override bool _performRightToLeftManipulations()
        {
            return false;
        }
    }
}
