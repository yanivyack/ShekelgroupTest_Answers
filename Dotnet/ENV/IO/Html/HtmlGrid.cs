using System;
using System.Collections.Generic;
using ENV.IO.Html.Advanced;
using Firefly.Box.UI;

namespace ENV.IO.Html
{
    public class HtmlGrid : HtmlElement
    {
        public ControlBorderStyle Border { get; set; }
        public int RowHeight { get; set; }
        public int HeaderHeight { get; set; }


        public bool HeaderOnEveryPage { get; set; }
        public bool FixedSize { get; set; }
        public readonly List<HtmlGridColumn> Columns = new List<HtmlGridColumn>();
        internal override void WriteTo(HtmlContentWriter.myWriter writer)
        {
            writer.WriteOpenTag("TABLE", "border", 2, "bgcolor", "#C0C0C0");
            writer.Indent();
            writer.WriteOpenTag("TR", "bgcolor", "#808080");
            writer.Indent();
            foreach (var htmlGridColumn in Columns)
            {
                htmlGridColumn.WriteHeader(writer);
            }
            writer.UnIndent();
            writer.WriteCloseTag("TR");
            writer.UnIndent();
            writer.SetGridWriter(w =>
            {
                w.Indent();
                w.WriteOpenTag("TR");
                w.Indent();
                foreach (var htmlGridColumn in Columns)
                {
                    htmlGridColumn.WriteBody(w);
                }
                w.UnIndent();
                w.WriteCloseTag("TR");
                w.UnIndent();
            });

            writer.WriteCloseTag("TABLE");
        }
        public override void AdvanceLastTop(Action<int> advance)
        {
            advance(2);
        }

        public override void AdvanceLastTopBeforeBr(Action<int> advance)
        {
            advance(1);
        }

        internal override void Prepare()
        {
            foreach (var htmlGridColumn in Columns)
            {
                htmlGridColumn.Prepare();
            }
        }
    }

    public class HtmlGridColumn
    {
        public HtmlElementCollection Elements = new HtmlElementCollection(false, false);
        public HtmlElementCollection HeaderElements = new HtmlElementCollection(false, false);

        public string Text { get; set; }
        public int Width { get; set; }
        internal void WriteHeader(HtmlContentWriter.myWriter writer)
        {
            using (writer.DisableNewLine())
            {
                writer.WriteOpenTag("TD", "valign", new HtmlAttributeValue("top"), "width", Width);
                using (var c = writer.GetContentListener())
                {
                    HeaderElements.WriteTo(writer);
                    if (!c.ContentWasWriten)
                        writer.WriteEmptySpace();
                }
                writer.WriteCloseTag("TD");

            }
            writer.WriteLine();
        }

        internal void WriteBody(HtmlContentWriter.myWriter writer)
        {
            using (writer.DisableNewLine())
            {
                writer.WriteOpenTag("TD", "valign", new HtmlAttributeValue("top"), "width", Width);
                using (var c = writer.GetContentListener())
                {
                    Elements.WriteTo(writer);
                    if (!c.ContentWasWriten)
                        writer.WriteEmptySpace();
                }
                writer.WriteCloseTag("TD");

            }
            writer.WriteLine();
        }

        internal void Prepare()
        {
            HeaderElements.Prepare();
            Elements.Prepare();
        }
    }
}