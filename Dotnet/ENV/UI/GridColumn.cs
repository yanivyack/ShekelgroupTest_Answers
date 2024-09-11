using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENV.Labs;
using Firefly.Box.Data.Advanced;
using Firefly.Box.UI;
using System.ComponentModel;

namespace ENV.UI
{
    public class GridColumn : Firefly.Box.UI.GridColumn
    {
        public GridColumn()
        {

        }

        protected override string Translate(string term)
        {
            return base.Translate(ENV.Languages.Translate(term));
        }
        public void AddFilterOption(string name, FilterBase where, bool defaultActive = false)
        {
            AddFilterOption(name, () => where, defaultActive);
        }
        public void AddFilterOption(string name, Func<FilterBase> where, bool defaultActive = false)
        {
            _filterOptions.Add(new FilterOption(name, where, defaultActive));
        }
        List<FilterOption> _filterOptions = new List<FilterOption>();
        class FilterOption
        {
            private string _name;
            private Func<FilterBase> _where;
            private bool _defaultActive;

            public FilterOption(string name, Func<FilterBase> where, bool defaultActive)
            {
                _name = name;
                _where = where;
                _defaultActive = defaultActive;
            }

            internal void Apply(Action<string, Func<FilterBase>, bool> add)
            {
                add(_name, _where, _defaultActive);
            }
        }

        internal void ProvideFilterOptionsTo(Action<string, Func<FilterBase>, bool> add)
        {
            foreach (var item in _filterOptions)
            {
                item.Apply(add);
            }
        }

        public override Firefly.Box.UI.FontScheme FontScheme
        {
            get
            {
                return base.FontScheme;
            }
            set
            {
                base.FontScheme = FaceLiftDemo.MatchFontScheme(value);
            }
        }
        bool _userVisible = true;
        public bool UserVisible
        {
            get { return _userVisible; }
            set
            {
                _userVisible = value;
                if (UserVisibleChanged != null)
                    UserVisibleChanged(_userVisible);
                _userVisibleChangingDeveloperVisible = true;
                Visible = _developerVisible && _userVisible;
                _userVisibleChangingDeveloperVisible = false;
            }
        }
        bool _userVisibleChangingDeveloperVisible = false;
        bool _developerVisible = true;
        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                if (!_userVisibleChangingDeveloperVisible)
                {
                    _developerVisible = value;
                    if (DeveloperVisibleChanged != null)
                        DeveloperVisibleChanged(_developerVisible);
                }
                base.Visible = value && _userVisible;
            }
        }
        public event Action<bool> DeveloperVisibleChanged, UserVisibleChanged;

        public bool AllowFilter { get; set; }
        [System.ComponentModel.DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int OriginalLeft { get; internal set; }

        protected override void OnLoad()
        {
            base.OnLoad();

            if (AllowFilter)
            {
                var gridHeaderBackColor = System.Drawing.Color.Empty;
                var gridHeaderForeColor = System.Drawing.Color.Empty;
                HeaderButton.Enabled = true;
                {
                    var g = Parent as Grid;
                    if (g != null)
                    {
                        gridHeaderBackColor = g.ColumnHeadersBackColor;
                        gridHeaderForeColor = g.ColumnHeadersForeColor;
                    }

                    HeaderButton.PreferredSize = new System.Drawing.Size(18, g != null ?
                        Math.Max(0, g.HeaderHeight - (gridHeaderBackColor != System.Drawing.Color.Empty && g.Border == ControlBorderStyle.None ? 1 : 2)) : 16);
                }

                HeaderButton.HideSortStateIndicator = true;
                HeaderButton.SeparatorInsteadOfBorder = true;

                if (System.Windows.Forms.Application.RenderWithVisualStyles)
                {
                    HeaderButton.BackColor = gridHeaderBackColor != System.Drawing.Color.Empty ? gridHeaderBackColor : System.Drawing.SystemColors.GradientInactiveCaption;
                    HeaderButton.ForeColor = gridHeaderForeColor != System.Drawing.Color.Empty ? gridHeaderForeColor : System.Drawing.SystemColors.ActiveCaptionText;
                    HeaderButton.HotTrackForeColor = gridHeaderForeColor != System.Drawing.Color.Empty ? gridHeaderForeColor : System.Drawing.SystemColors.ActiveCaptionText;
                    HeaderButton.HotTrackBackColor = gridHeaderBackColor != System.Drawing.Color.Empty ?
                        System.Drawing.Color.FromArgb((int)(gridHeaderBackColor.R * 0.9), (int)(gridHeaderBackColor.G * 0.9), (int)(gridHeaderBackColor.B * 0.9)) : System.Drawing.Color.LightSkyBlue;
                    HeaderButton.BorderColor = HeaderButton.HotTrackBorderColor = System.Drawing.SystemColors.ControlLight;
                }

                HeaderButton.Click +=
                    (o, eventArgs) =>
                    {
                        Firefly.Box.UI.Advanced.InputControlBase icb = null;
                        Firefly.Box.Data.Advanced.ColumnBase col = null;
                        foreach (var control in Controls)
                        {
                            icb = control as Firefly.Box.UI.Advanced.InputControlBase;
                            if (icb != null)
                            {
                                var x = Common.GetColumn(icb);
                                if (x != null)//W6695 - Make sure to get the right control on the gc - not the first one.
                                    col = x;
                            }
                        }


                        var g = Parent as Grid;
                        var bounds = new System.Drawing.Rectangle(Location.X, 0, Width, g != null ? g.HeaderHeight : 0);
                        if (g != null)
                            bounds.Offset(g.Location);

                        Grid.DoOnCurrentUIController(
                            uic =>
                            {
                                uic.Raise(Commands.GridColumnFilterClick, Text, col != null ? (int)ENV.UserMethods.Instance.IndexOf(col) : 0, bounds.X, bounds.Y, bounds.Width, bounds.Height);
                            }, this);
                    };
            }
        }

        internal bool WidthChangedDueToBindWidth;
        protected override void OnWidthChangedDueToBindWidth()
        {
            base.OnWidthChangedDueToBindWidth();
            WidthChangedDueToBindWidth = true;
        }
    }
}
