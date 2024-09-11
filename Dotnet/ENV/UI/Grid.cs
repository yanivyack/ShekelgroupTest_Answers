using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ENV.Labs;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data;
using Firefly.Box.Data.Advanced;
using Firefly.Box.UI;
using Firefly.Box.UI.Advanced;
using ButtonState = System.Windows.Forms.ButtonState;

namespace ENV.UI
{
    public partial class Grid : Firefly.Box.UI.Grid
    {
        static Firefly.Box.UI.ColorScheme DefaultActiveRowColorScheme = new Firefly.Box.UI.ColorScheme(SystemColors.HighlightText, SystemColors.Highlight);
        public Grid()
        {
            GridColumnType = typeof(GridColumn);
            DefaultTextBoxType = typeof(TextBox);
            ActiveRowColorScheme = DefaultActiveRowColorScheme;
            LastColumnHeaderSeparator = true;
            StandardPaging = false;
            IgnoreAdvancedAnchorWidthWhenNoVisibleAutoResizeColumns = true;

        }

        internal event Action Load;
        bool _gridLoaded = false;
        bool _wasSetStartOnRowPosition = false;
        public override GridStartOnRowPosition StartOnRowPosition
        {
            get { return base.StartOnRowPosition; }
            set
            {
                _wasSetStartOnRowPosition = true;
                base.StartOnRowPosition = value;
            }
        }

        protected override void OnLoad()
        {

            _gridLoaded = true;
            if (Load != null)
            {
                Load();
            }

            if (UseDefaultBackColorForStandardStyleWithRowColorStyleByColumnAndControls &&
                Style == Firefly.Box.UI.ControlStyle.Standard && RowColorStyle == Firefly.Box.UI.GridRowColorStyle.ByColumnAndControls)
                BackColor = SystemColors.Window;
            FaceLiftDemo.GridLoaded(this);

            if (AlwaysUseUnderConstructionNewGridLook)
                UnderConstructionNewGridLook = true;
            if (AlwaysUseUnderConstructionNewGridLook || AlwaysEnableGridEnhancements || EnableGridEnhancementsCodeSample)
                EnableGridEnhancements();
            if (Style != ControlStyle.Standard || !UseVisualStyles)
                DrawPartialRowContent = false;
            if (ActiveRowColorScheme == DefaultActiveRowColorScheme && ActiveRowStyle == GridActiveRowStyle.RowBackColor && UseControlAsActiveRowBackColorWhenStyleIsRowBackColorAndActiveRowColorSchemeWasNotSet)
                ActiveRowColorScheme = new MissingColor();
            foreach (var item in Controls)
            {
                var gc = item as ENV.UI.GridColumn;
                if (gc != null)
                    gc.OriginalLeft = gc.DeferredLeft;
            }

            var f = FindForm();
            if (f != null)
            {
                if (f.RightToLeft == RightToLeft.Yes && RightToLeft == RightToLeft.No && AdvancedAnchor.RightPercentage == 100 && AdvancedAnchor.LeftPercentage == 100)
                    AdvancedAnchor.LeftPercentage = 0;
                if (!_wasSetStartOnRowPosition)
                {
                    var ff = f as ENV.UI.Form;
                    if (ff != null)
                    {
                        ff.RunInLogicContext(() => {
                            this.StartOnRowPosition = UserSettings.GridStartOnRowPosition;
                        });
                    }

                }
            }

            base.OnLoad();

        }
        public bool UseDefaultBackColorForStandardStyleWithRowColorStyleByColumnAndControls = false;
        public bool UseControlAsActiveRowBackColorWhenStyleIsRowBackColorAndActiveRowColorSchemeWasNotSet = false;
        internal static void DoOnControls(ControlCollection controls, Action<TextControlBase> doOnControl)
        {
            foreach (System.Windows.Forms.Control control in controls)
            {
                var cb = control as TextControlBase;
                if (cb != null)
                    doOnControl(cb);
                DoOnControls(control.Controls, doOnControl);
            }
        }
        Dictionary<Color, ColorScheme> _schemeCache = new Dictionary<Color, ColorScheme>();

        public override ColorScheme ActiveRowColorScheme
        {
            get
            {
                return base.ActiveRowColorScheme;
            }
            set
            {

                if (value != null && value.BackColor == Color.Transparent)
                {
                    ColorScheme result;
                    if (!_schemeCache.TryGetValue(value.ForeColor, out result))
                    {
                        _schemeCache.Add(value.ForeColor, result = new ColorScheme(value.ForeColor, SystemColors.ControlLight));

                    }
                    value = result;
                }
                base.ActiveRowColorScheme = value;
            }
        }
        protected override Type GetControlTypeForWizard(ColumnBase column)
        {
            var col = column as IENVColumn;
            if (col != null && col.ControlTypeOnGrid != null)
                return col.ControlTypeOnGrid;
            return base.GetControlTypeForWizard(column);
        }
        public bool EnableGridEnhancementsCodeSample { get; set; }
        public static bool AlwaysUseUnderConstructionNewGridLook = false;
        protected override string Translate(string term)
        {
            return base.Translate(ENV.Languages.Translate(term));
        }


        #region Grid Enhancements
        public static bool AlwaysEnableGridEnhancements = false;

        interface IColumnCaster
        {
            void DoOnColumn<dataType>(TypedColumnBase<dataType> column);
            void DoOnColumnBase(ColumnBase column);
        }

        class Caster : UserMethods.IColumnSpecifier
        {
            IColumnCaster _caster;

            public Caster(IColumnCaster caster)
            {
                _caster = caster;
            }

            public void DoOnColumn(TypedColumnBase<Text> column)
            {
                _caster.DoOnColumn(column);
            }

            public void DoOnColumn(TypedColumnBase<Number> column)
            {
                _caster.DoOnColumn(column);
            }

            public void DoOnColumn(TypedColumnBase<Date> column)
            {
                _caster.DoOnColumn(column);
            }

            public void DoOnColumn(TypedColumnBase<Time> column)
            {
                _caster.DoOnColumn(column);
            }

            public void DoOnColumn(TypedColumnBase<Bool> column)
            {
                _caster.DoOnColumn(column);
            }

            public void DoOnColumn(TypedColumnBase<byte[]> column)
            {

            }

            public void DoOnUnknownColumn(ColumnBase column)
            {
                _caster.DoOnColumnBase(column);
            }
        }
        static void CastColumn(ColumnBase column, IColumnCaster caster)
        {
            UserMethods.CastColumn(column, new Caster(caster));
        }


        public static void DoOnCurrentUIController(Action<Firefly.Box.UIController> what, System.Windows.Forms.Control c)
        {
            ENV.Common.RunOnLogicContext(c, () =>
                                            {
                                                var uic = Firefly.Box.Context.Current.ActiveTasks[Firefly.Box.Context.Current.ActiveTasks.Count - 1] as UIController;
                                                if (uic != null)
                                                    what(uic);
                                            });


        }
        bool _enabledGridEnhancements = false;
        public void EnableGridEnhancements()
        {
            if (_enabledGridEnhancements)
                return;
            _enabledGridEnhancements = true;
            new GridHelper(this);
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == 0x7B) //WM_CONTEXTMENU
            {
                if (_gridEnhancementsContextMenuStrip != null)
                {
                    if (this.ContextMenuStrip == null || PointToClient(new Point(0, (int)m.LParam >> 16)).Y <= HeaderHeight)
                    {
                        var old = _contextMenuStripSet ? this.ContextMenuStrip : null;
                        try
                        {
                            this.ContextMenuStrip = _gridEnhancementsContextMenuStrip;
                            base.WndProc(ref m);
                        }
                        finally
                        {
                            this.ContextMenuStrip = null;
                        }
                        return;
                    }
                }
            }
            base.WndProc(ref m);
        }

        public override ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return base.ContextMenuStrip;
            }

            set
            {
                if (value != null && value != _gridEnhancementsContextMenuStrip)
                    _contextMenuStripSet = true;
                base.ContextMenuStrip = value;
            }
        }
        bool _contextMenuStripSet;
        ContextMenuStrip _gridEnhancementsContextMenuStrip;

        public static event Action<Action<string, Action>> AddToGridContext;

        class GridHelper
        {
            ENV.UI.Grid _g;
            ContextMenuStrip cm;
            CustomCommand _copySelectedData;
            public GridHelper(ENV.UI.Grid g)
            {
                _g = g;
                _g.AllowUserToReorderColumns = true;
                _g.AllowUserToResizeColumns = true;



                /*
                g.TemporaryPaint += new Action<PaintEventArgs>(g_Paint);
                g.MouseMove += g_MouseHover;
                */

                cm = new ContextMenuStrip();

                {
                    var excel = new ToolStripMenuItem("Export To Excel");
                    cm.Items.Add(excel);
                    excel.Click += delegate
                    {
                        DoOnCurrentUIController(ENV.Labs.GridExports.ExportToExcel, _g);
                    };
                }
                {
                    var tab = new ToolStripMenuItem("Copy Grid Data");
                    cm.Items.Add(tab);
                    tab.Click += delegate
                    {
                        DoOnCurrentUIController(GridExports.GridToClipBoard, _g);
                    };
                }
                if (AddToGridContext != null)
                {
                    AddToGridContext((s, action) =>
                    {
                        var tab = new ToolStripMenuItem(s);
                        cm.Items.Add(tab);
                        tab.Click += delegate
                                     {
                                         ENV.Common.RunOnLogicContext(_g, action);

                                     };
                    });
                }
                {

                }
                {
                    var tab = new ToolStripMenuItem("Copy Screen");
                    cm.Items.Add(tab);
                    tab.Click += delegate
                    {
                        DoOnCurrentUIController(x =>
                        {
                            var f = x.View;
                            while (f.Parent is Firefly.Box.UI.Form)
                                f = f.Parent as Firefly.Box.UI.Form;


                            {
                                Firefly.Box.Context.Current.InvokeUICommand(() =>
                                {
                                    var bp = new Bitmap(f.Width, f.Height);
                                    f.DrawToBitmap(bp,
                                                          new System.Drawing.Rectangle(0, 0, bp.Width,
                                                                                       bp.Height));
                                    Clipboard.SetImage(bp);
                                    bp.Dispose();
                                });
                            }

                        }, _g);
                    };
                }

                var columnsToolItem = new ToolStripMenuItem("Columns");
                cm.Items.Add(columnsToolItem);
                _g._gridEnhancementsContextMenuStrip = cm;
                if (_g._gridLoaded)
                    OnGridLoad(columnsToolItem);
                else
                    _g.Load += () => OnGridLoad(columnsToolItem);

                var filter = new ToolStripMenuItem("Filter");
                cm.Items.Add(filter);
                FilterOnAllColumns(filter);
            }

            void FilterOnAllColumns(ToolStripMenuItem filter)
            {
                var x = new FilterOnAllColumnsClass(_g);
                filter.Click +=
                    delegate
                    {

                        x.Filter(y => filter.Checked = y);


                    };
            }
            bool _loaded = false;
            private void OnGridLoad(ToolStripMenuItem columnsToolItem)
            {
                if (_loaded)
                    return;
                _g.WidthByColumns = false;
                _loaded = true;
                foreach (var item in _g.Controls)
                {
                    var gc = item as ENV.UI.GridColumn;
                    if (gc != null)
                    {
                        gc.AllowSort = true;
                        var mi = new ToolStripMenuItem(gc.Text);
                        columnsToolItem.DropDownItems.Add(mi);
                        mi.Checked = gc.UserVisible;
                        gc.DeveloperVisibleChanged += x => mi.Available = x;
                        gc.UserVisibleChanged += x => mi.Checked = x;
                        mi.Click += delegate
                        {
                            gc.UserVisible = !gc.UserVisible;
                            _g.FindForm().Invalidate();
                        };

                        new ColumnHelper(this, gc).Init();
                    }
                }
                var f = _g.FindForm() as ENV.UI.Form;
                if (f != null && !string.IsNullOrEmpty(f.UserStateIdentifier))
                {
                    columnsToolItem.DropDownItems.Add(new ToolStripSeparator());
                    columnsToolItem.DropDownItems.Add("RestoreDefaults").Click += delegate { Form.FormStateClear(f.UserStateIdentifier); };
                }
            }


            class ColumnHelper
            {
                ENV.UI.GridColumn _gridColumn;
                GridHelper _parent;
                ContextMenuStrip _cm;
                ToolStripMenuItem _fc;
                ColumnBase _col;
                DynamicFilter df = null;

                public ColumnHelper(GridHelper parent, ENV.UI.GridColumn gc)
                {
                    _parent = parent;
                    _gridColumn = gc;
                    _cm = new ContextMenuStrip();

                }

                public void Init()
                {
                    if (_col != null)
                        return;
                    InputControlBase icb = null;
                    foreach (var control in _gridColumn.Controls)
                    {
                        icb = control as Firefly.Box.UI.Advanced.InputControlBase;
                        if (icb != null)
                        {
                            var col = Common.GetColumn(icb);
                            if (col != null)
                            {
                                _col = col;

                            }
                        }
                    }
                    _gridColumn.ProvideFilterOptionsTo((name, filter, defaultOn) => AddFilterOption(name, c => filter(), defaultOn));

                    if (_col == null)
                        return;


                    AddFilterOption("Filter On Current Value", x =>
                    {
                        var eq = new GetEqualToColumn();
                        CastColumn(_col, eq);
                        return eq.Result;
                    });
                    UserMethods.CastColumn(_col, new AddTypeSpecificFilter(this, icb));
                    _cm.Items.Add("Hide Column").Click += delegate { _gridColumn.UserVisible = false; };

                    _gridColumn.HeaderButton.Enabled = true;
                    _gridColumn.HeaderButton.Click +=
                        (o, eventArgs) =>
                        {
                            _cm.Show(_parent._g.PointToScreen(new Point(_gridColumn.HeaderButton.Bounds.Left, _gridColumn.HeaderButton.Bounds.Bottom)));
                        };
                }


                class AddTypeSpecificFilter : UserMethods.IColumnSpecifier
                {
                    ColumnHelper _parent;
                    InputControlBase _icb;

                    public AddTypeSpecificFilter(ColumnHelper parent, InputControlBase icb)
                    {
                        _parent = parent;
                        _icb = icb;
                    }

                    public void DoOnColumn(TypedColumnBase<Text> column)
                    {
                        _parent.AddFilterOption("Custom Filter",
                            uic => new CustomFilterDialog<Text>(new TextColumn(), new TextColumn(), uic, column).Run());
                        _parent.AddFilterOption("Select Values",
                               uic =>
                               {
                                   FilterBase fc = null;
                                   var location = new Point(_parent._rect.Left, _parent._rect.Bottom);
                                   Firefly.Box.Context.Current.InvokeUICommand(() =>
                                   {
                                       location = EnhancedFeatures.PointToMdiClient(_parent._parent._g.PointToScreen(location));
                                   });
                                   fc = new MultiSelectFilter<Text>(uic, column, location, new TextColumn() { Format = column.Format }, _icb).Run();
                                   return fc;

                               });


                    }

                    public void DoOnColumn(TypedColumnBase<Number> column)
                    {
                        _parent.AddFilterOption("Custom Filter",
                            x => new CustomFilterDialog<Number>(new NumberColumn(), new NumberColumn(), x, column).Run());
                        _parent.AddFilterOption("Select Values",
                            uic =>
                            {
                                var location = new Point(_parent._rect.Left, _parent._rect.Bottom);
                                Firefly.Box.Context.Current.InvokeUICommand(()=>
                                    location = EnhancedFeatures.PointToMdiClient(_parent._parent._g.PointToScreen(location)));
                                FilterBase fc = new MultiSelectFilter<Number>(uic, column, location, new NumberColumn() { Format = column.Format }, _icb).Run();
                                return fc;

                            });

                    }

                    public void DoOnColumn(TypedColumnBase<Date> column)
                    {
                        _parent.AddFilterOption("Custom Filter",
                            x => new CustomFilterDialog<Date>(new DateColumn(), new DateColumn(), x, column).Run());
                        _parent.AddFilterOption("Select Values",
                            uic =>
                            {
                                var location = new Point(_parent._rect.Left, _parent._rect.Bottom);
                                location = EnhancedFeatures.PointToMdiClient(_parent._parent._g.PointToScreen(location));
                                FilterBase fc = new MultiSelectFilter<Date>(uic, column, location, new DateColumn() { Format = column.Format }, _icb).Run();
                                return fc;

                            });
                    }

                    public void DoOnColumn(TypedColumnBase<Time> column)
                    {
                        _parent.AddFilterOption("Custom Filter",
                            x => new CustomFilterDialog<Time>(new TimeColumn(), new TimeColumn(), x, column).Run());
                        _parent.AddFilterOption("Select Values",
                            uic =>
                            {
                                var location = new Point(_parent._rect.Left, _parent._rect.Bottom);
                                location = EnhancedFeatures.PointToMdiClient(_parent._parent._g.PointToScreen(location));
                                FilterBase fc = new MultiSelectFilter<Time>(uic, column, location, new TimeColumn() { Format = column.Format }, _icb).Run();
                                return fc;

                            });
                    }

                    public void DoOnColumn(TypedColumnBase<Bool> column)
                    {
                        _parent.AddFilterOption("Select Values",
                       uic =>
                       {
                           var location = new Point(_parent._rect.Left, _parent._rect.Bottom);
                           location = EnhancedFeatures.PointToMdiClient(_parent._parent._g.PointToScreen(location));
                           FilterBase fc = new MultiSelectFilter<Bool>(uic, column, location, new BoolColumn() { Format = column.Format }, _icb).Run();
                           return fc;

                       });
                    }

                    public void DoOnColumn(TypedColumnBase<byte[]> column)
                    {

                    }

                    public void DoOnUnknownColumn(ColumnBase column)
                    {

                    }
                }


                Action<FilterCollection> _applyFilter = delegate { };

                void AddFilterOption(string name, Func<UIController, FilterBase> createFilter, bool defaultOn = false)
                {
                    var item = new ToolStripMenuItem(name);
                    _cm.Items.Add(item);
                    if (defaultOn)
                    {
                        DoOnCurrentUIController((uic) =>
                        {

                            var f = createFilter(uic);
                            if (f != null)
                            {
                                _applyFilter = y => y.Add(f);
                                foreach (var i in _cm.Items)
                                {
                                    var m = i as ToolStripMenuItem;
                                    if (m != null)
                                        m.Checked = false;
                                }
                                item.Checked = true;
                                _gridColumn.HeaderButton.Pinned = true;
                            }
                            if (df == null)
                            {
                                df = new DynamicFilter(z =>
                                {
                                    _applyFilter(z);
                                });
                                uic.Where.Add(df);
                            }
                        }, _gridColumn);
                    }
                    item.Click +=
                        delegate
                        {
                            ENV.Common.RunOnLogicContext(_gridColumn,
                                () =>
                                {
                                    DoOnColumn((uic, x, col) =>
                                    {
                                        if (item.Checked)
                                        {
                                            _applyFilter = delegate { };
                                            Context.Current.InvokeUICommand(() =>
                                            {
                                                item.Checked = false;
                                                _gridColumn.HeaderButton.Pinned = false;
                                            });
                                        }
                                        else
                                        {
                                            var f = createFilter(uic);
                                            if (f != null)
                                            {

                                                _applyFilter = y => y.Add(f);
                                                Context.Current.InvokeUICommand(
                                                    () =>
                                                    {
                                                        foreach (var i in _cm.Items)
                                                        {
                                                            var m = i as ToolStripMenuItem;
                                                            if (m != null)
                                                                m.Checked = false;
                                                        }
                                                        item.Checked = true;
                                                        _gridColumn.HeaderButton.Pinned = true;
                                                    });

                                            }
                                        }

                                        if (df == null)
                                        {
                                            df = new DynamicFilter(z =>
                                            {
                                                _applyFilter(z);
                                            });
                                            uic.Where.Add(df);
                                        }
                                        x.ReloadData();
                                    });
                                });
                        };
                }

                void DoOnColumn(Action<UIController, Firefly.Box.Advanced.UIOptions, ColumnBase> what)
                {
                    DoOnCurrentUIController(uic =>
                    {
                        if (uic != null)
                        {
                            uic.SaveRowAndDo(
                                x =>
                                {
                                    ProvideColumn(c => what(uic, x, c));
                                });
                        }
                    }, _gridColumn);

                }
                void ProvideColumn(Action<ColumnBase> to)
                {
                    foreach (var control in _gridColumn.Controls)
                    {
                        var icb =
                            control as
                            Firefly.Box.UI.Advanced.InputControlBase;
                        if (icb != null)
                        {
                            var col = Common.GetColumn(icb);
                            if (col != null)
                            {
                                to(col);
                                return;
                            }

                        }
                    }
                }

                class GetEqualToColumn : IColumnCaster
                {
                    public FilterBase Result = null;
                    public void DoOnColumn<dataType>(TypedColumnBase<dataType> column)
                    {
                        Result = column.IsEqualTo(column.Value);
                    }

                    public void DoOnColumnBase(ColumnBase column)
                    {
                        var t = column.GetType();
                        while (t != typeof(object))
                        {
                            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(TypedColumnBase<>))
                            {
                                foreach (var m in t.GetMethods())
                                {
                                    if (m.Name == "IsEqualTo")
                                    {
                                        if (t.GetGenericArguments()[0] == m.GetParameters()[0].ParameterType)
                                        {
                                            Result = (FilterBase)m.Invoke(column, new object[] { column.Value });
                                            return;
                                        }
                                    }
                                }
                            }
                            else
                                t = t.BaseType;

                        }


                    }
                }

                Rectangle _rect;
                bool _hoverred = false;
                public void Paint(PaintEventArgs obj)
                {
                    if (_gridColumn.Visible)
                    {
                        _rect = new Rectangle(_gridColumn.Right - 16, 2, 16, _parent._g.HeaderHeight - 4);

                        int w = 6;
                        if (_hoverred)
                        {
                            var p = Brushes.LightGray;
                            var sf = new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near };
                            obj.Graphics.DrawString(((char)0x76).ToString(), new Font("Marlett", 12), p, _rect, sf);
                        }

                    }
                }

                public void MouseHover(Point point)
                {
                    var h = new Rectangle(_gridColumn.Bounds.X, 0, _gridColumn.Width, _parent._g.HeaderHeight).Contains(point);
                    if (h != _hoverred)
                    {
                        _hoverred = h;
                        var r2 = new Rectangle(_rect.Location, _rect.Size);
                        //                        r2.Inflate(-2, -2);
                        _parent._g.Invalidate(r2);
                        _parent._g.Update();
                    }
                }

                public bool MouseDown(Point point)
                {
                    if (_rect.Contains(point))
                    {
                        _cm.Show(_parent._g.PointToScreen(new Point(_rect.Left, _rect.Bottom)));
                        return true;
                    }
                    return false;
                }

                public void SendColumnTo(Action<ColumnBase> action)
                {
                    throw new NotImplementedException();
                }
            }

            List<ColumnHelper> _columns;






        }
        #endregion
        bool _newGridLook = false;
        public bool UnderConstructionNewGridLook
        {
            get { return _newGridLook; }
            set
            {
                _newGridLook = value;
                Style = Firefly.Box.UI.ControlStyle.Standard;
                UseVisualStyles = true;
            }
        }
        public static Color HeaderGradiantColor1 = Color.FromArgb(221, 236, 254), HeaderGradiantColor2 = Color.FromArgb(136, 174, 228),
            HeaderHoverColor1 = Color.FromArgb(255, 240, 194), HeaderHoverColor2 = Color.FromArgb(255, 214, 155);
        class CustomGridPainter : IGridPainterUnderConstructionInterfaceWILLCHANGE
        {
            Grid _parent;

            public CustomGridPainter(Grid parent)
            {
                _parent = parent;
            }

            public Image DrawColumnTitle(Rectangle columnRect, bool showColumnSeparator, bool mouseOver, bool mousePressed)
            {
                //  return Image.FromFile(@"c:\1.bmp");
                Image result = new Bitmap(columnRect.Width, columnRect.Height);
                using (var g = Graphics.FromImage(result))
                {
                    var r = new Rectangle(Point.Empty, columnRect.Size);
                    using (var b =
                        mousePressed
                            ? new System.Drawing.Drawing2D.LinearGradientBrush(r, Color.FromArgb(254, 142, 75),
                                                                               Color.FromArgb(254, 201, 133), 90)
                            : mouseOver
                                  ? (Brush)
                                    new System.Drawing.Drawing2D.LinearGradientBrush(r, HeaderHoverColor1,
                                                                                     HeaderHoverColor2, 90)
                                  : new System.Drawing.Drawing2D.LinearGradientBrush(r,
                                                                                     HeaderGradiantColor1,
                                                                                     HeaderGradiantColor2,
                                                                                     90))
                    {
                        g.FillRectangle(b, r);
                    }
                    var linePosition = r.Width - 1;
                    var line2Position = r.Width - 2;
                    if (_parent.RightToLeft == RightToLeft.Yes)
                    {
                        linePosition = 0;
                        line2Position = 1;
                    }
                    if (showColumnSeparator)
                    {
                        using (var p = new Pen(Color.FromArgb(106, 140, 203)))
                        {
                            g.DrawLine(p, new Point(line2Position, 2), new Point(line2Position, r.Height - 2));
                        }



                        g.DrawLine(Pens.White, new Point(linePosition, 3), new Point(linePosition, r.Height - 1));
                    }
                }
                return result;
            }
        }
        protected override Firefly.Box.UI.Grid.IGridPainterUnderConstructionInterfaceWILLCHANGE GetGridPainter()
        {
            if (UnderConstructionNewGridLook)
                return new CustomGridPainter(this);
            return base.GetGridPainter();
        }

        public class ButtonsGridScrollBar : Firefly.Box.UI.Advanced.GridScrollBarBase
        {
            IGridScrollBarScrollingHandler _scrollingHandler;

            Area _start, _pageUp, _up, _down, _pageDown, _end;

            System.Collections.Generic.List<Area> _areas
            {
                get { return new List<Area>(new[] { _start, _pageUp, _up, _down, _pageDown, _end }); }
            }

            public ButtonsGridScrollBar()
            {
                SetStyle(System.Windows.Forms.ControlStyles.AllPaintingInWmPaint | System.Windows.Forms.ControlStyles.Opaque |
                    System.Windows.Forms.ControlStyles.UserPaint | System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer, true);

                _scrollingHandler = new myScrollingHandler();
                Font = new Font("Webdings", 10);

                _start = new Area((args, rect, buttonState) => DrawButtonWithRotatedChar(args.Graphics, rect, buttonState, 0x39), () => _scrollingHandler.Start());
                _pageUp = new Area((args, rect, buttonState) => DrawButtonWithRotatedChar(args.Graphics, rect, buttonState, 0x37), () => _scrollingHandler.LargeDecrement());
                _up = new Area((args, rect, buttonState) => ControlPaint.DrawScrollButton(args.Graphics, rect, ScrollButton.Up, buttonState), () => _scrollingHandler.SmallDecrement());
                _down = new Area((args, rect, buttonState) => ControlPaint.DrawScrollButton(args.Graphics, rect, ScrollButton.Down, buttonState), () => _scrollingHandler.SmallIncrement());
                _pageDown = new Area((args, rect, buttonState) => DrawButtonWithRotatedChar(args.Graphics, rect, buttonState, 0x38), () => _scrollingHandler.LargeIncrement());
                _end = new Area((args, rect, buttonState) => DrawButtonWithRotatedChar(args.Graphics, rect, buttonState, 0x3A), () => _scrollingHandler.End());

                ResetAreas();

                _pressTimer.Tick +=
                    (sender, e) =>
                    {
                        if (_pressedArea != null)
                        {
                            if (_pressedArea.Rectangle.Contains(PointToClient(MousePosition)))
                                _pressedArea.Pressed();
                        }
                        _pressTimer.Interval = 20;
                    };
            }

            void DrawButtonWithRotatedChar(Graphics g, Rectangle rect, ButtonState state, int chr)
            {
                ControlPaint.DrawButton(g, rect, state);
                var format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                format.FormatFlags = StringFormatFlags.DirectionVertical;
                g.DrawString(new string(new[] { (char)chr }), Font, state == ButtonState.Inactive ? SystemBrushes.ControlDark : SystemBrushes.ControlText, rect, format);
            }

            new delegate void Paint(PaintEventArgs e, Rectangle rect, System.Windows.Forms.ButtonState state);

            class Area
            {
                public Area(Paint paint, Action pressed)
                {
                    Paint = paint;
                    Pressed = pressed;
                }

                public Rectangle Rectangle;
                public Paint Paint;
                public Action Pressed;
            }


            class myScrollingHandler : IGridScrollBarScrollingHandler
            {
                public void SmallDecrement()
                {
                }

                public void SmallIncrement()
                {
                }

                public void LargeDecrement()
                {
                }

                public void LargeIncrement()
                {
                }

                public void Start()
                {
                }

                public void End()
                {
                }
            }

            Area _pressedArea = null;

            void ResetAreas()
            {
                var buttonMinHeight = 17;
                var clientRect = ClientRectangle;
                _start.Rectangle = new Rectangle(0, 0, clientRect.Width, clientRect.Height >= buttonMinHeight * 6 ? buttonMinHeight : 0);
                _end.Rectangle = new Rectangle(0, clientRect.Bottom - buttonMinHeight, clientRect.Width, clientRect.Height >= buttonMinHeight * 6 ? buttonMinHeight : 0);

                _pageUp.Rectangle = new Rectangle(0, _start.Rectangle.Height, clientRect.Width, clientRect.Height >= buttonMinHeight * 4 ? buttonMinHeight : 0);
                _pageDown.Rectangle = new Rectangle(0, clientRect.Bottom - _end.Rectangle.Height - buttonMinHeight, clientRect.Width, clientRect.Height >= buttonMinHeight * 4 ? buttonMinHeight : 0);

                var remainingHeight = clientRect.Height - _start.Rectangle.Height - _pageUp.Rectangle.Height - _pageDown.Rectangle.Height - _end.Rectangle.Height;
                _up.Rectangle = new Rectangle(0, _start.Rectangle.Height + _pageUp.Rectangle.Height, clientRect.Width, remainingHeight / 2);
                _down.Rectangle = new Rectangle(0, _up.Rectangle.Bottom, clientRect.Width, remainingHeight - _up.Rectangle.Height);
            }

            Timer _pressTimer = new Timer();

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _pressTimer.Dispose();
                }
                base.Dispose(disposing);
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                ResetAreas();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                _areas.ForEach(
                    area =>
                    {
                        if (area.Rectangle.Height > 0)
                            area.Paint(e, area.Rectangle, Enabled ? (_pressedArea == area ? ButtonState.Pushed : ButtonState.Normal) : ButtonState.Inactive);
                    });
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);

                Area newPressedArea = null;
                _areas.ForEach(
                    area =>
                    {
                        if (area.Rectangle.Contains(e.Location))
                        {
                            newPressedArea = area;
                        }
                    });
                if (newPressedArea != null)
                {
                    _pressedArea = newPressedArea;
                    Invalidate(newPressedArea.Rectangle);
                    Update();
                    newPressedArea.Pressed();
                    _pressTimer.Interval = 400;
                    _pressTimer.Start();
                }
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);
                StopPress();
            }

            protected override void OnMouseCaptureChanged(EventArgs e)
            {
                base.OnMouseCaptureChanged(e);
                if (!this.Capture)
                    StopPress();
            }

            void StopPress()
            {
                _pressTimer.Stop();
                if (_pressedArea != null)
                {
                    _pressedArea = null;
                    Refresh();
                }
            }


            public override void SetScrollingHandler(IGridScrollBarScrollingHandler scrollingHandler)
            {
                _scrollingHandler = scrollingHandler;
            }

            public override int Minimum { get; set; }
            public override int Maximum { get; set; }
            public override int Value { get; set; }
            public override int LargeChange { get; set; }
            public override bool ExtraUpperTrack { get; set; }
            public override bool ExtraLowerTrack { get; set; }
            public override bool UseVisualStyles { get; set; }
        }

        [Browsable(false)]
        public int LeftForBindLeftOfControlsOnGrid
        {
            get
            {
                var x = DeferredLeft;
                if (!HorizontalScrollbar)
                    x -= GetAutoScrollPosition().X;
                return x;
            }
        }

        Point GetAutoScrollPosition()
        {
            var p = Parent as ScrollableControl;
            if (p != null) return p.AutoScrollPosition;
            return Point.Empty;
        }
    }
    class FilterOnAllColumnsClass
    {
        FilterCollection myFilterCollection = null;
        Grid _g;
        public FilterOnAllColumnsClass(Grid g)
        {
            _g = g;
        }
        bool _filtered = false;
        public void Filter(Action<bool> setFiltered)
        {
            Grid.DoOnCurrentUIController(
                uic =>
                {
                    uic.SaveRowAndDo(opt =>
                                     {
                                         if (myFilterCollection == null)
                                         {
                                             myFilterCollection = new FilterCollection();
                                             uic.Where.Add(myFilterCollection);
                                         }
                                         if (_filtered)
                                         {
                                             myFilterCollection.Clear();
                                             _filtered = false;
                                             setFiltered(false);
                                             opt.ReloadData();
                                         }
                                         else
                                         {
                                             var columns = new List<TextColumn>();
                                             foreach (var controls in _g.Controls)
                                             {
                                                 var gc = controls as Firefly.Box.UI.GridColumn;
                                                 if (gc != null)
                                                 {
                                                     foreach (var cc in gc.Controls)
                                                     {
                                                         var icb = cc as InputControlBase;
                                                         if (icb != null)
                                                         {
                                                             var col = Common.GetColumn(icb);
                                                             if (col != null)
                                                             {
                                                                 var c = col as Firefly.Box.Data.TextColumn;
                                                                 if (c != null)
                                                                 {
                                                                     columns.Add(c);
                                                                 }
                                                             }
                                                         }
                                                     }
                                                 }
                                             }
                                             if (columns.Count > 0)
                                             {
                                             }
                                             var result = new CustomFilterDialog<Text>(
                                                 new TextColumn(), new TextColumn(), uic, columns.ToArray()).Run();
                                             if (result != null)
                                             {
                                                 myFilterCollection.Add(result);
                                                 _filtered = true;
                                                 setFiltered(true);
                                                 opt.ReloadData();
                                             }
                                         }
                                     });
                }, _g);
        }
    }
}
