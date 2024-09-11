using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ENV.IO.Html.Advanced;
using Firefly.Box.UI;
using Firefly.Box.UI.Advanced;
using HtmlElement = ENV.IO.Html.Advanced.HtmlElement;

namespace ENV.IO.Html
{
    public class HtmlLabel : HtmlElement
    {
        public string Text { get; set; }
        public string Href { get; set; }
        public bool Multiline { get; set; }
        internal override void WriteTo(HtmlContentWriter.myWriter writer)
        {
            if (String.IsNullOrEmpty(Href))
                writer.WriteText(Text, ColorScheme, FontScheme);
            else
            {
                using (writer.DisableNewLine())
                {
                    writer.WriteOpenTag("A", "href", Href);
                    writer.WriteText(Text, null, FontScheme);
                    writer.WriteCloseTag("A");
                }
                writer.WriteNewLine();
            }
        }

    }

    public class HtmlTable : HtmlElement
    {
        public int Border { get; set; }
        public int CellSpacing { get; set; }
        public int CellPadding { get; set; }
        public List<HtmlTableRow> Rows = new List<HtmlTableRow>(), _rows = new List<HtmlTableRow>();
        internal override void WriteTo(HtmlContentWriter.myWriter writer)
        {
            var attributes = new List<object>();
            attributes.AddRange(new object[] { "border", Border * 4 / 5, "cellspacing", CellSpacing * 4 / 5, "cellpadding", CellPadding * 4 / 5 });

            if (writer.ShouldSerializeBackColor(ColorScheme))
            {
                attributes.AddRange(new object[] { "bgcolor", writer.GenerateColorString(ColorScheme.BackColor) });
            }
            using (writer.ColorContext(ColorScheme))
            {
                writer.WriteOpenTag("TABLE", attributes.ToArray());
                writer.Indent();
                foreach (var htmlGridColumn in _rows)
                {
                    htmlGridColumn.WriteTo(writer);
                }
                writer.UnIndent();
                writer.WriteCloseTag("TABLE");
            }

        }

        internal override void Prepare()
        {
            _rows = new List<HtmlTableRow>(Rows);
            _rows.Sort((a, b) => a.Top.CompareTo(b.Top));
            foreach (var row in _rows)
            {
                row.Prepare();
            }
        }
        public override void AdvanceLastTop(Action<int> advance)
        {
            advance(3);
        }

        public override void AdvanceLastTopBeforeBr(Action<int> advance)
        {
            //     advance(1);
        }
    }
    public class HtmlTableRow
    {
        public List<HtmlTableCell> Cells = new List<HtmlTableCell>(), _cells = new List<HtmlTableCell>();
        public virtual ColorScheme ColorScheme { get; set; }
        public FontScheme FontScheme { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }
        public ContentAlignment Alignment { get; set; }
        public bool Visible { get; set; }
        public string Text { get; set; }
        public HtmlTableRow()
        {
            Visible = true;
        }

        public void BindVisible(Func<bool> expression)
        {
            _onWrite.Add(() => Visible = expression());
        }
        internal static void AddAlignmentToAttributes(ContentAlignment alignment, List<object> attributes)
        {

            switch (alignment)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.BottomLeft:
                    break;
                case ContentAlignment.BottomCenter:
                case ContentAlignment.TopCenter:
                case ContentAlignment.MiddleCenter:
                    attributes.Add("align");
                    attributes.Add(new HtmlAttributeValue("center"));
                    break;
                case ContentAlignment.MiddleRight:
                case ContentAlignment.BottomRight:
                case ContentAlignment.TopRight:
                    attributes.Add("align");
                    attributes.Add(new HtmlAttributeValue("right"));
                    break;
            }
            switch (alignment)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.TopCenter:
                case ContentAlignment.TopRight:
                    attributes.Add("valign");
                    attributes.Add(new HtmlAttributeValue("top"));
                    break;
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.MiddleRight:
                    break;
                case ContentAlignment.BottomLeft:
                case ContentAlignment.BottomCenter:
                case ContentAlignment.BottomRight:

                    break;
            }
        }
        List<Action> _onWrite = new List<Action>();
        internal void WriteTo(HtmlContentWriter.myWriter writer)
        {
            _onWrite.ForEach(a => a());
            if (!Visible)
                return;
            var y = new List<object>();
            AddAlignmentToAttributes(Alignment, y);
            if (!string.IsNullOrEmpty(Text))
            {
                y.Add("background");
                y.Add(Text);
            }
            writer.WriteOpenTag("TR", y.ToArray());
            writer.Indent();
            foreach (var htmlTableCell in _cells)
            {
                htmlTableCell.WriteTo(writer);
            }
            writer.UnIndent();
            writer.WriteCloseTag("TR");
        }

        public void Prepare()
        {
            _cells = new List<HtmlTableCell>(Cells);
            _cells.Sort((a, b) => a.Left.CompareTo(b.Left));
            foreach (var htmlTableCell in _cells)
            {
                htmlTableCell.Prepare();
            }

        }
    }
    public class HtmlTableCell
    {
        public HtmlTableCell()
        {
            Visible = true;
            Width = -1;
        }

        public HtmlElementCollection Elements = new HtmlElementCollection(false, false);
        public virtual ColorScheme ColorScheme { get; set; }
        public FontScheme FontScheme { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }
        public ContentAlignment Alignment { get; set; }
        public bool Visible { get; set; }
        public string Text { get; set; }
        public void BindVisible(Func<bool> expression)
        {
            _onWrite.Add(() => Visible = expression());
        }
        List<Action> _onWrite = new List<Action>();

        internal void WriteTo(HtmlContentWriter.myWriter writer)
        {
            foreach (var action in _onWrite)
            {
                action();
            }
            using (writer.DisableNewLine())
            {
                var attributes = new List<object>();
                if (writer.ShouldSerializeBackColor(ColorScheme))
                {

                    attributes.Add("bgcolor");
                    attributes.Add(writer.GenerateColorString(ColorScheme.BackColor));
                }
                Html.HtmlTableRow.AddAlignmentToAttributes(Alignment, attributes);
                if (Width != -1)
                    attributes.Add("width");
                attributes.Add(Width*4/5);
                if (!string.IsNullOrEmpty(Text))
                {
                    attributes.Add("background");
                    attributes.Add(Text);
                }
                writer.WriteOpenTag("TD", attributes.ToArray());


                using (var c = writer.GetContentListener())
                {
                    Elements.Prepare();
                    Elements.WriteTo(writer);
                    if (!c.ContentWasWriten)
                        writer.WriteEmptySpace();
                }
                writer.WriteCloseTag("TD");

            }
            writer.WriteLine();
        }

        public void Prepare()
        {
            Elements.Prepare();
        }
    }
    public class HtmlTextBox : HtmlElement
    {
        public string Format { get; set; }
        public ControlData Data { get; set; }
        public bool Multiline { get; set; }
        public bool UseSystemPasswordChar { get; set; }
        public void BindFormat(Func<string> expression)
        {
            _onWrite.Add(() => Format = expression());
        }

        internal override void WriteTo(HtmlContentWriter.myWriter writer)
        {
            writer.WriteText(Data.ToString(Format), ColorScheme, FontScheme);
        }
    }
    public class HtmlLine : HtmlElement
    {
        public HtmlLine()
        {
            WidthPrecentage = 100;
        }


        public int LineWidth { get; set; }
        public int WidthPrecentage { get; set; }
        internal override void WriteTo(HtmlContentWriter.myWriter writer)
        {
            writer.WriteOpenTag("HR", "align", Alignment, "size", LineWidth, "width",
                                new HtmlAttributeValue(WidthPrecentage + "%"));
        }
        internal override bool ISerializeMyOwnAlignment()
        {
            return true;
        }

    }

    public enum RelativeContentAlignment
    {
        Top,
        Bottom,
        Center,
        Left,
        Right
    }
    public class HtmlPictureBox : HtmlElement
    {
        public HtmlPictureBox()
        {
            RelativeContentAlignment = Html.RelativeContentAlignment.Bottom;
        }

        public ControlData Data { get; set; }
        public string ImageLocation { get; set; }
        public RelativeContentAlignment RelativeContentAlignment { get; set; }
        internal override void WriteTo(HtmlContentWriter.myWriter writer)
        {
            writer.WriteOpenTag("IMG", "src", ImageLocation, "align", RelativeContentAlignment, "alt", ImageLocation);
        }

        public override void AdvanceLastTop(Action<int> advance)
        {
        }
    }
    namespace Advanced
    {
        public class HtmlElement
        {
            public virtual ColorScheme ColorScheme { get; set; }
            public FontScheme FontScheme { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int Top { get; set; }
            public int Left { get; set; }
            public HorizontalAlignment Alignment { get; set; }
            public bool Visible { get; set; }
            protected List<Action> _onWrite = new List<Action>();
            public void BindVisible(Func<bool> expression)
            {
                _onWrite.Add(() => Visible = expression());
            }
            public void BindFontScheme(Func<FontScheme> expression)
            {
                _onWrite.Add(() => FontScheme = expression());
            }
            public void BindColorScheme(Func<ColorScheme> expression)
            {
                _onWrite.Add(() => ColorScheme = expression());
            }
            public HtmlElement()
            {
                Visible = true;
            }


            internal virtual void WriteTo(HtmlContentWriter.myWriter writer)
            {
            }

            internal virtual bool ISerializeMyOwnAlignment()
            {
                return false;
            }

            public virtual void AdvanceLastTop(Action<int> advance)
            {
                advance(1);
            }

            public virtual void AdvanceLastTopBeforeBr(Action<int> advance)
            {

            }

            internal virtual void Prepare()
            {
                foreach (var action in _onWrite)
                {
                    action();
                }
            }
        }
    }
}
