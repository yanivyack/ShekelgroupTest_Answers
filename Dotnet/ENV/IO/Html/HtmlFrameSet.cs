using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Firefly.Box;
using Firefly.Box.Advanced;

namespace ENV.IO.Html
{
    public class HtmlFrameSet
    {
        ITask _task;
        UserMethods u;

        public HtmlFrameSet()
        {
            RelativeSizes = true;
        }

        public HtmlFrameSet(AbstractUIController task)
            : this()
        {
            _task = task._uiController;
            u = task.u;
        }
        public HtmlFrameSet(BusinessProcessBase task)
            : this()
        {
            _task = task._businessProcess;
            u = task.u;
        }
        public HtmlFrameSet(ApplicationControllerBase task)
            : this()
        {
            _task = task._moduleController;
            u = task.u;
        }
        public HtmlFrameSet(Firefly.Box.BusinessProcess task, UserMethods u)
        {
            _task = task;
            this.u = u;
        }
        
        public string Text { get; set; }
        public bool Border { get; set; }
        public int Spacing { get; set; }
        public bool RelativeSizes { get; set; }

        public readonly HtmlFrameCollection Frames = new HtmlFrameCollection();
        TextTemplate _myTextTemplate;
        public void WriteTo(WebWriter web)
        {
            GetTextTemplate().WriteTo(web);
        }

        string _content = "THE CONTENT";
        TextTemplate GetTextTemplate()
        {
            if (_myTextTemplate == null)
            {
                _myTextTemplate = new TextTemplate(new StringReader(
                    @"
<HTML>

<HEAD>
<META name=""generator"" content=""Migrated Frame Set"">
<TITLE><!$MG_TITLE></TITLE>

</HEAD>

<!$MGREPEAT>
<!$MG_CONTENT>
<!$MGENDREPEAT>

</HTML>
"));
                _myTextTemplate.Add(new Tag("TITLE", Text));
                _myTextTemplate.Add(new Tag("CONTENT", () => _content));
            }

            using (var sw = new StringWriter())
            {
                sw.Write("<FRAMESET frameborder={0} framespacing={1} ", Border ? 1 : 0, Spacing);
                var s = Frames.GetProportions(RelativeSizes);
                sw.Write(s + ">");
                foreach (HtmlFrame frame in Frames)
                {
                    sw.WriteLine();
                    sw.Write("\t");
                    frame.WriteTo(sw, RelativeSizes);
                }
                sw.WriteLine("</FRAMESET>");


                _content = sw.ToString();
            }


            return _myTextTemplate;
        }



        public void WriteTo(FileWriter file)
        {
            GetTextTemplate().WriteTo(file);

        }
    }

    public class HtmlFrame
    {
        public string Name { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public bool Scroll { get; set; }
        public bool Resize { get; set; }
        public bool Vertical { get; set; }
        public string PublicName { get; set; }
        public string Url { get; set; }

        public readonly HtmlFrameCollection Frames = new HtmlFrameCollection();
        protected List<Action> _onWrite = new List<Action>();
        public void BindName(Func<Text> expression)
        {
            _onWrite.Add(() => Name = expression());
        }
        public void BindUrl(Func<Text> expression)
        {
            _onWrite.Add(() => Url = expression());
        }

        public void WriteTo(StringWriter sw, bool relativeSizes)
        {
            foreach (var action in _onWrite)
            {
                action();
            }
            if (Frames.Count > 0)
            {
                sw.WriteLine("<FRAMESET {0}>", Frames.GetProportions(relativeSizes));
                foreach (HtmlFrame frame in Frames)
                {
                    sw.Write("\t\t");
                    frame.WriteTo(sw, relativeSizes);
                }
                sw.WriteLine("\t</FRAMESET>");
            }
            else
            {
                sw.Write("<FRAME src=\"{0}\"", (Url ?? "").Replace("\"", "\"\""));
                if (!string.IsNullOrEmpty(Name))
                    sw.Write(" name =\"{0}\"", Name);
                if (!Resize)
                    sw.Write(" noresize");
                if (!Scroll)
                    sw.Write(" scrolling=no");
                sw.WriteLine(">");
            }
        }
    }

    public class HtmlFrameCollection : IEnumerable<HtmlFrame>
    {
        public int Count { get { return _frames.Count; } }
        List<HtmlFrame> _frames = new List<HtmlFrame>();
        public void Add(HtmlFrame frame)
        {
            _frames.Add(frame);
        }

        IEnumerator<HtmlFrame> IEnumerable<HtmlFrame>.GetEnumerator()
        {
            return _frames.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return _frames.GetEnumerator();
        }
        internal string GetProportions(bool relativeSizes)
        {
            string s = null;
            bool vertical = false;
            int total = 0;
            Number lastValue = -1;
            bool hasVal = false;
            foreach (HtmlFrame frame in _frames)
            {
                if (s == null)
                {
                    vertical = frame.Vertical;
                    if (vertical)
                    {
                        s = "cols=\"";
                    }
                    else s = "rows=\"";
                    if (relativeSizes)
                    {
                        foreach (HtmlFrame f in _frames)
                        {
                            if (vertical)
                                total += f.Width;
                            else
                                total += f.Height;
                        }
                    }
                }
                if (lastValue != -1)
                {
                    var x = lastValue.ToString();

                    if (relativeSizes)
                    {
                        x = ENV.UserMethods.Instance.Round(lastValue / total * 100, 3, 0).ToString().Trim() + "%";
                    }
                    if (hasVal)
                        s += ",";
                    else
                        hasVal = true;
                    s += x;
                }
                if (vertical)
                {
                    lastValue = frame.Width;
                }
                else lastValue = frame.Height;
            }
            if (s != null)
            {
                if (hasVal)
                    s += ",";
                s += "*\"";
            }
            return s;
        }
    }
}
