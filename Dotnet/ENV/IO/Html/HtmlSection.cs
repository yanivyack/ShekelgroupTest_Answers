using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ENV.IO.Html.Advanced;
using Firefly.Box.Advanced;
using Firefly.Box.UI;
using HtmlElement = ENV.IO.Html.Advanced.HtmlElement;

namespace ENV.IO.Html
{
    public class HtmlSection
    {
      
        public void WriteTo(WebWriter web)
        {
            web.Write(this);
        }
        public void WriteTo(FileWriter file)
        {
            file.Write(this);
        }
        public HtmlSection()
        {

        }

        ITask _task;
        UserMethods u;
        public HtmlSection(AbstractUIController task)
            : this()
        {
            _task = task._uiController;
            u = task.u;
        }
        public HtmlSection(BusinessProcessBase task)
            : this()
        {
            _task = task._businessProcess;
            u = task.u;
        }
        public HtmlSection(ApplicationControllerBase task)
            : this()
        {
            _task = task._moduleController;
            u = task.u;
        }
        public HtmlSection(Firefly.Box.BusinessProcess task, UserMethods u)
        {
            _task = task;
            this.u = u;
        }

        public FontScheme DefaultFontScheme { get; set; }
        public FontScheme H1FontScheme { get; set; }
        public FontScheme H2FontScheme { get; set; }
        public FontScheme H3FontScheme { get; set; }
        public FontScheme H4FontScheme { get; set; }
        public FontScheme H5FontScheme { get; set; }
        public FontScheme H6FontScheme { get; set; }
        public FontScheme BoldFontScheme { get; set; }
        public FontScheme H1BoldFontScheme { get; set; }
        public FontScheme H2BoldFontScheme { get; set; }
        public FontScheme H3BoldFontScheme { get; set; }
        public FontScheme H4BoldFontScheme { get; set; }
        public FontScheme H5BoldFontScheme { get; set; }
        public FontScheme H6BoldFontScheme { get; set; }
        public FontScheme ItalicFontScheme { get; set; }
        public FontScheme H1ItalicFontScheme { get; set; }
        public FontScheme H2ItalicFontScheme { get; set; }
        public FontScheme H3ItalicFontScheme { get; set; }
        public FontScheme H4ItalicFontScheme { get; set; }
        public FontScheme H5ItalicFontScheme { get; set; }
        public FontScheme H6ItalicFontScheme { get; set; }
        public FontScheme BoldItalicFontScheme { get; set; }
        public FontScheme H1BoldItalicFontScheme { get; set; }
        public FontScheme H2BoldItalicFontScheme { get; set; }
        public FontScheme H3BoldItalicFontScheme { get; set; }
        public FontScheme H4BoldItalicFontScheme { get; set; }
        public FontScheme H5BoldItalicFontScheme { get; set; }
        public FontScheme H6BoldItalicFontScheme { get; set; }

        public string HeaderFile { get; set; }
        List<Action> _onWrite = new List<Action>();
        public void BindHeaderFile(Func<string> expression)
        {
            _onWrite.Add(() => HeaderFile = expression());
        }
        public void BindText(Func<string> expression)
        {
            _onWrite.Add(() => Text = expression());
        }
        public ColorScheme ColorScheme { get; set; }
        public FontScheme FontScheme { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }
        public string Text { get; set; }
        public string BackgroundImageLocation { get; set; }



        public HtmlElementCollection Elements = new HtmlElementCollection(true, true);

        internal void DoWrite(HtmlContentWriter.myWriter writer)
        {
            foreach (var action in _onWrite)
            {
                action();
            }
            Elements.Prepare();
            Elements.WriteTo(writer);
        }

    }
    public class HtmlElementCollection
    {
        List<HtmlElement> _elements = new List<HtmlElement>(), _sortedElements = new List<HtmlElement>();
        bool _doBr = false;
        bool _doAlign = false;
        internal HtmlElementCollection(bool doBr, bool doAlign)
        {
            _doAlign = doAlign;
            _doBr = doBr;
        }

        public void Add(HtmlElement element)
        {
            _elements.Add(element);
        }
        internal void Prepare()
        {
            _sortedElements = new List<HtmlElement>(_elements);
            _sortedElements.Sort((a, b) =>
            {
                var i = a.Top.CompareTo(b.Top);
                if (i == 0)
                    i = a.Left.CompareTo(b.Left);
                return i;
            });
            foreach (var sortedElement in _sortedElements)
            {
                sortedElement.Prepare();
            }
        }

        internal void WriteTo(HtmlContentWriter.myWriter writer)
        {

            int lastTop = 1;
            var lastAlignment = HorizontalAlignment.Left;
            string alignmentTagToClose = null;
            foreach (var htmlControlBase in _sortedElements)
            {
                if (!htmlControlBase.Visible)
                    continue;
                if (!htmlControlBase.ISerializeMyOwnAlignment())
                {
                    if (htmlControlBase.Alignment != lastAlignment)
                    {
                        if (lastAlignment != HorizontalAlignment.Left)
                            writer.WriteCloseTag(alignmentTagToClose);
                    }
                }
                htmlControlBase.AdvanceLastTopBeforeBr(i => lastTop += i);
                while (lastTop < htmlControlBase.Top)
                {
                    lastTop++;
                    if (_doBr)
                        writer.WriteOpenTag("BR");
                }
                if (!htmlControlBase.ISerializeMyOwnAlignment() && _doAlign)
                {

                    if (htmlControlBase.Alignment != lastAlignment)
                    {

                        writer.WriteLine();
                        if (htmlControlBase.Alignment == HorizontalAlignment.Right)
                        {
                            alignmentTagToClose = "DIV";
                            writer.WriteOpenTag(alignmentTagToClose, "align", htmlControlBase.Alignment);
                            
                        }
                        else
                            if (htmlControlBase.Alignment == HorizontalAlignment.Center)
                            {
                                alignmentTagToClose = "CENTER";
                                writer.WriteOpenTag(alignmentTagToClose);
                                

                            }
                        lastAlignment = htmlControlBase.Alignment;
                    }
                }

                htmlControlBase.WriteTo(writer);
                htmlControlBase.AdvanceLastTop(i => lastTop += i);

            }
            if (lastAlignment != HorizontalAlignment.Left)
            {
                writer.WriteCloseTag(alignmentTagToClose);
                writer.WriteLine();
            }
        }
    }
}