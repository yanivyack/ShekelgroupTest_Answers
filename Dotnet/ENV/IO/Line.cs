using System;
using System.Drawing;
using ENV.IO.Advanced;
using Firefly.Box.UI.Advanced;

namespace ENV.IO
{
    [ToolboxBitmap(typeof(Firefly.Box.UI.Line))]
    public class Line : ENV.IO.Advanced.TextControlBase
    {
        public Line()
        {
            Multiline = true;
        }
        protected override string Translate(string term)
        {
            return base.Translate(ENV.Languages.Translate(term));
        }
        internal override void WriteTo(Write writer)
        {

            if (HeightInChars == 1)
                writer(new string(Start.Y == End.Y ? '-' : '|', WidthInChars), true);

            else
            {
                var s = "";
                for (int i = 0; i < HeightInChars ; i++)
                {
                    s += "|";
                }
                writer(s, true);
            }
        }

        internal override void SetYourValue(string value, IStringToByteArrayConverter c)
        {

        }

        internal override bool _performRightToLeftManipulations()
        {
            return false;
        }

        Point _start = new Point(0, 0), _end = new Point();

        public Point Start
        {
            get { return _start; }
            set
            {
                _start = value;
                SetPosition();
            }
        }
        void SetPosition()
        {
            var location = new Point(Math.Min(_start.X, _end.X),
                                          Math.Min(_start.Y, _end.Y));
            var size = new Size(Math.Abs(_start.X - _end.X) + 1,
                                   Math.Abs(_start.Y - _end.Y) + 1);

            base.LeftInChars = location.X;
            base.TopInChars = location.Y;
            base.HeightInChars = size.Height;
            base.WidthInChars = size.Width;

        }
        public event BindingEventHandler<IntBindingEventArgs> BindStart_X
        {
            add { AddBindingEvent("Start_X", () => Start.X, x => Start = new Point(x, Start.Y), value); }
            remove { RemoveBindingEvent("Start_X", value); }
        }
        public event BindingEventHandler<IntBindingEventArgs> BindStart_Y
        {
            add { AddBindingEvent("Start_Y", () => Start.Y, x => Start = new Point(Start.X, x), value); }
            remove { RemoveBindingEvent("Start_Y", value); }
        }
        public event BindingEventHandler<IntBindingEventArgs> BindEnd_X
        {
            add { AddBindingEvent("End_X", () => End.X, x => End = new Point(x, End.Y), value); }
            remove { RemoveBindingEvent("End_X", value); }
        }
        public event BindingEventHandler<IntBindingEventArgs> BindEnd_Y
        {
            add { AddBindingEvent("End_Y", () => End.Y, x => End = new Point(End.X, x), value); }
            remove { RemoveBindingEvent("End_Y", value); }
        }

        public Point End
        {
            get { return _end; }
            set
            {
                _end = value;
                SetPosition();
            }
        }

    }
}
