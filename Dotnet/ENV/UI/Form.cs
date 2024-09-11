using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using ENV.BackwardCompatible;
using ENV.Labs;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;
using Firefly.Box.UI;
using Firefly.Box.UI.Advanced;
using CancelEventArgs = System.ComponentModel.CancelEventArgs;

namespace ENV.UI
{
    [System.ComponentModel.Designer(typeof(ENV.UI.Form.FormDesigner), typeof(IRootDesigner))]
    public class Form : Firefly.Box.UI.Form, ICanShowCustomHelp
    {
        public class FormDesigner : Firefly.Box.UI.Form.FormDesigner
        {
            public override void BuildDesigner(Firefly.Box.UI.Designer.IDesignerBuilder<Firefly.Box.UI.Form> builder)
            {
                ENV.AbstractFactory.WindowsFormsDesignMode = true;
                base.BuildDesigner(builder);
                builder.AddAction("Add Controls to Toolbox...", () =>
                {
                    try
                    {
                        var uiService = (System.Windows.Forms.Design.IUIService)GetService(typeof(System.Windows.Forms.Design.IUIService));
                        var form = new ToolboxControlsDialog((IToolboxService)GetService(typeof(IToolboxService)), x => CreateTool(x));


                        var d = new System.Windows.Forms.OpenFileDialog();
                        d.DefaultExt = "dll";
                        d.Filter = "Assemblies|*.dll;*.exe|All files|*.*";
                        d.AddExtension = true;
                        d.RestoreDirectory = true;
                        if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            form.Clear();
                            form.AddAssembly(d.FileName, true);
                            //form.FillTree();

                            //var path = System.Windows.Forms.Application.ExecutablePath;
                            //foreach (var dll in System.IO.Directory.GetFiles(Path.GetDirectoryName(path), "*.dll"))
                            //{
                            //  form.AddAssembly(dll);
                            //}
                            uiService.ShowDialog(form);
                        }



                    }
                    catch (Exception ex)
                    {
                        ENV.Common.ShowExceptionDialog(ex, true, "");
                    }

                });
                builder.AddAction("Setup Toolbox", () =>
                {
                    var uiService = (System.Windows.Forms.Design.IUIService)GetService(typeof(System.Windows.Forms.Design.IUIService));
                    if (
                        uiService.ShowMessage("Do you want to add all controls to the Visual Studio Toolbox?",
                            "Setup Toolbox", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        var toolboxService = (IToolboxService)GetService(typeof(IToolboxService));
                        var t = Instance.GetType();
                        while (t.BaseType.FullName != typeof(ENV.UI.Form).FullName)
                        {
                            t = t.BaseType;
                        }
                        var ss = t.Assembly;
                        var uiNamespace = t.Namespace;
                        {
                            var printingNS = uiNamespace.Remove(uiNamespace.LastIndexOf(".")) + ".Printing";
                            foreach (var c in new[]
                         {
                            "TextBox",
                            "Label",
                            "Grid",
                            "GroupBox",
                            "PictureBox",
                            "RichTextBox",
                            "Shape",
                            "Line",
                            "GridColumn",
                        })
                            {
                                try
                                {
                                    var item = new ToolboxItem(ss.GetType(printingNS + "." + c));
                                    toolboxService.AddToolboxItem(item, printingNS);
                                }
                                catch
                                {
                                }

                            }
                        }
                        {
                            var TextIONS = uiNamespace.Remove(uiNamespace.LastIndexOf(".")) + ".TextIO";
                            foreach (var c in new[]
                            {
                            "TextBox",
                            "TextLabel",
                            "Line",
                            "Shape",

                        })
                            {
                                try
                                {
                                    var item = new ToolboxItem(ss.GetType(TextIONS + "." + c));
                                    toolboxService.AddToolboxItem(item, TextIONS);
                                }
                                catch
                                {
                                }

                            }
                        }
                        foreach (var c in new[]
                        {
                            "TextBox",
                            "Label",
                            "Button",
                            "Grid",
                            "SubForm",
                            "CheckBox",
                            "ComboBox",

                            "GroupBox",
                            "ListBox",
                            "PictureBox",
                            "RadioButton",
                            "TabControl",
                            "RichTextBox",
                            "TreeView",
                            "Shape",
                            "Line",
                            "GridColumn",
                            "ScrollBar",
                            "ActiveX"
                        })
                        {
                            try
                            {
                                var item = new ToolboxItem(ss.GetType(uiNamespace + "." + c));
                                toolboxService.AddToolboxItem(item, uiNamespace);
                            }
                            catch
                            {
                            }
                        }
                        {
                            var item = new ToolboxItem(typeof(Firefly.Box.UI.Advanced.ControlExtender));
                            toolboxService.AddToolboxItem(item, uiNamespace);
                        }
                        toolboxService.SetSelectedToolboxItem(null);
                    }
                });
            }
        }

        protected override bool FilterTypesForChangeControlTypes(Type controlType)
        {
            if (controlType.Assembly == typeof(UI.TextBox).Assembly)
                return false;
            var bt = controlType.BaseType;
            while (bt != null)
            {
                if (bt.Namespace.StartsWith(typeof(UI.TextBox).Namespace))
                    return true;
                bt = bt.BaseType;
            }
            return false;
        }
        public override ContextMenuStrip ContextMenuStrip
        {
            get
            {
                var r = base.ContextMenuStrip;
                if (r != null && r.Items.Count == 0)
                {
                    var z = r as ENV.UI.Menus.ContextMenuStripBase;
                    if (z != null)
                    {
                        z.InitMenus();
                    }
                }
                return r;
            }

            set
            {
                base.ContextMenuStrip = value;
            }
        }

        public static bool DisableMouseWheel = false;
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (DisableMouseWheel)
                return;
            base.OnMouseWheel(e);
        }

        public Form()
        {
            ShowInTaskbar = false;
            MouseWheelScrollsSingleRow = false;
        }


        static Form()
        {
            StealKeyboardFocusFromForeignWindows = true;
        }

        protected string ConvertToZeroBasedListOfValues(string values)
        {
            string result = "";
            foreach (var value in values.Split(','))
            {
                var n = (Number.Parse(value) - 1);
                if (n >= 0)
                {
                    if (result.Length > 0)
                        result += ", ";
                    if (!string.IsNullOrEmpty(value.Trim()))
                        result += n.ToString();
                }
            }
            return result;
        }


        protected override System.Drawing.Image GetImage(string imageLocation)
        {
            return ENV.Common.GetImage(imageLocation);
        }
        internal protected System.Windows.Forms.Control FindControlByTag(string controlTag)
        {
            return FindControlByTag(this, controlTag);
        }

        static System.Windows.Forms.Control FindControlByTag(System.Windows.Forms.Control parent, string controlTag)
        {
            if (string.IsNullOrEmpty(controlTag))
                return null;

            for (var i = parent.Controls.Count - 1; i >= 0; i--)
            {
                var control = parent.Controls[i];
                if (control is System.Windows.Forms.Form)
                    continue;
                var result = control;
                if (result != null && string.Equals(controlTag, (result.Tag ?? "").ToString(), StringComparison.InvariantCultureIgnoreCase))
                    return result;
                result = FindControlByTag(control, controlTag);
                if (result != null)
                    return result;
            }
            return null;
        }
        public bool AutoUserStateIdentifier { get; set; }
        public string UserStateIdentifier { get; set; }
        public event BindingEventHandler<StringBindingEventArgs> BindUserStateIdentifier;

        internal int GetAverageCharWidth(Font font)
        {
            return (int)GetAverageCharSize(font).Width;
        }

        protected override string Translate(string term)
        {
            return base.Translate(ENV.Languages.Translate(term));
        }
        [System.ComponentModel.DefaultValue(false)]
        public new bool ShowInTaskbar
        {
            get { return base.ShowInTaskbar; }
            set { base.ShowInTaskbar = value; }
        }

        double _horizontalScale = 1, _verticalScale = 1, _horizontalExpressionFactor = 1, _verticalExpressionFactor = 1;
        [DefaultValue(1)]
        public double HorizontalScale
        {
            get { return _horizontalScale * _scalingFactor.Width; }
            set { _horizontalScale = value; }
        }
        [DefaultValue(1)]
        public double VerticalScale
        {
            get { return _verticalScale * _scalingFactor.Height; }
            set { _verticalScale = value; }
        }
        [DefaultValue(1)]
        public double HorizontalExpressionFactor
        {
            get { return _horizontalExpressionFactor; }
            set { _horizontalExpressionFactor = value; }
        }
        [DefaultValue(1)]
        public double VerticalExpressionFactor
        {
            get { return _verticalExpressionFactor; }
            set { _verticalExpressionFactor = value; }
        }

        public int ToPixelHorizontal(Number value)
        {
            return ToPixel(value, HorizontalScale, HorizontalExpressionFactor);
        }
        public int ToPixelVertical(Number value)
        {
            return ToPixel(value, VerticalScale, VerticalExpressionFactor);
        }
        internal static int ToPixel(Number value, double scale, double expressionFactor)
        {
            if (value == null)
                return 0;
            var i = Math.Truncate((double)value * (double)expressionFactor);
            return (int)Math.Round(i / expressionFactor * scale);
        }


        SizeF __scalingFactor = SizeF.Empty;
        SizeF _scalingFactor
        {
            get
            {
                if (DesignMode) return new SizeF(1, 1);
                if (__scalingFactor == SizeF.Empty)
                {
                    if (AutoScaleMode == AutoScaleMode.Font && !(_horizontalScale == 1 && _verticalScale == 1))
                    {
                        var fontSize = GetAverageCharSize(Font);
                        __scalingFactor = new SizeF((float)(fontSize.Width / _horizontalScale), (float)(fontSize.Height / _verticalScale));
                    }
                    else
                    {
                        using (var g = Graphics.FromHwnd(IntPtr.Zero))
                            __scalingFactor = new SizeF(g.DpiX / 96, g.DpiY / 96);
                    }
                    __scalingFactor = new SizeF(__scalingFactor.Width * ScalingFactor.Width, __scalingFactor.Height * ScalingFactor.Height);
                }
                return __scalingFactor;
            }
        }

        internal SizeF ScalingFactorInternal { get { return _scalingFactor; } }

        public static SizeF ScalingFactor = new SizeF(1, 1);
        static bool _fontScalingFactorSet;
        static SizeF _fontScalingFactor = new SizeF(1, 1);

        public static SizeF FontScalingFactor
        {
            get { return _fontScalingFactor; }
            set
            {
                _fontScalingFactor = value;
                _fontScalingFactorSet = true;
            }
        }

        public static FontScheme MatchFont(FontScheme value)
        {
            if (value == null)
                return null;
            if (ScalingFactor.Height == 1 && ScalingFactor.Width == 1 && FontScalingFactor.Height == 1 && FontScalingFactor.Width == 1)
                return value;
            var factor = _fontScalingFactorSet ? _fontScalingFactor : ScalingFactor;
            var x = factor.Width;
            if (factor.Height < x)
                x = factor.Height;
            return new FontScheme()
            {

                Font =
                    new Font(value.Font.OriginalFontName, value.Font.Size * x, value.Font.Style, value.Font.Unit, value.Font.GdiCharSet),
                TextAngle = value.TextAngle
            };

        }
        static Size OriginalSizeForScaling = Size.Empty;
        public static void SetOriginalSizeTo800X600()
        {
            SetOriginalSize(new Size(804, 489));
        }
        public static void SetOriginalSize(Size s)
        {
            OriginalSizeForScaling = s;
            _maxBottom = OriginalSizeForScaling.Height;
            _maxRight = OriginalSizeForScaling.Width;
        }
        public new AutoScaleMode AutoScaleMode { get; set; }

        static int _maxBottom = 0, _maxRight = 0, _mdiHeight = 0, _mdiWidth = 0;
        internal static void MatchScreenSize(Size s)
        {
            _mdiHeight = s.Height;
            _mdiWidth = s.Width;
            AutomaticResizeScreens = true;
            ScalingFactor = new SizeF((float)_mdiWidth / _maxRight, (float)_mdiHeight / _maxBottom);
        }
        public static void ToggleScalingDemo()
        {

            if (Control.ModifierKeys == Keys.Control)
            {

                Data.NumberColumn originalWidth = new Data.NumberColumn("Original Width", "5")
                {
                    Value = OriginalSizeForScaling.IsEmpty ? _maxRight : OriginalSizeForScaling.Width
                },
                    originalHeight = new Data.NumberColumn("Original Height", "5")
                    {
                        Value = OriginalSizeForScaling.IsEmpty ? _maxBottom : OriginalSizeForScaling.Height
                    };

                var fb = new FormBuilder("Scaling Config");
                fb.AddColumn(originalWidth);
                fb.AddColumn(originalHeight);
                fb.AddAction("DeActivate", () =>
                {
                    ScalingFactor = new SizeF(1, 1);
                    AutomaticResizeScreens = false;
                    fb.Close();
                });
                fb.AddAction("Activate", () =>
                {
                    OriginalSizeForScaling = new Size(originalWidth.Value, originalHeight);
                    SetOriginalSize(OriginalSizeForScaling);
                    MatchScreenSize(GetCurrentScreenSize());
                    MessageBox.Show("MDI Width: " + _mdiWidth + " Height: " + _mdiHeight + "\r\nFactor Width: " + ScalingFactor.Width + ", Height: " + ScalingFactor.Height);
                    fb.Close();
                });
                fb.Run();


            }
            else
            {
                if (AutomaticResizeScreens)
                {
                    ScalingFactor = new SizeF(1, 1);
                    AutomaticResizeScreens = false;
                    return;
                }
                if (OriginalSizeForScaling.IsEmpty)
                {
                    try
                    {
                        var iniValue = ENV.UserMethods.Instance.IniGet("Scaling");
                        if (!string.IsNullOrWhiteSpace(iniValue))
                        {
                            var sp = iniValue.ToString().Split('x');
                            SetOriginalSize(new Size(Number.Parse(sp[0]), Number.Parse(sp[1])));
                        }
                    }
                    catch { }
                }


                MatchScreenSize(GetCurrentScreenSize());
            }
        }

        private static Size GetCurrentScreenSize()
        {
            Size result = Size.Empty;
            var x = new UIController { View = new ENV.UI.Form { FitToMDI = true } };
            x.EnterRow += () =>
            {
                int width = 0;
                int height = 0;
                foreach (var item in Firefly.Box.Context.Current.ActiveTasks)
                {
                    if (item != x)
                    {
                        var v = item.View as ENV.UI.Form;
                        if (v != null)
                        {
                            if (v.Right > width)
                                width = v.Right;
                            if (v.Bottom > height)
                                height = v.Bottom;
                        }
                    }
                }
                height += 1;

                result = x.View.Size;
                x.Exit();
            };
            x.Run();
            return result;
        }

        internal static bool AutomaticResizeScreens;
        protected override void OnBeforeBinding()
        {
            base.OnBeforeBinding();
            var s = new Size(Right, Bottom);
            if (StartPosition != WindowStartPosition.Custom)
                s = new Size(Width, Height);
            if (OriginalSizeForScaling == Size.Empty && !FitToMDI)
            {
                if (s.Height > _maxBottom)
                    _maxBottom = s.Height;
                if (s.Width > _maxRight && s.Width < 1200)
                    _maxRight = s.Width;
            }
            if (AutomaticResizeScreens)
            {
                ScalingFactor = new SizeF((float)_mdiWidth / _maxRight, (float)_mdiHeight / _maxBottom);
            }
            if (_scalingFactor != new SizeF(1, 1))
                Scale(_scalingFactor);

            if (UserSettings.VersionXpaCompatible)
            {
                RunInLogicContext(() =>
                {
                    if (Context.Current.ActiveTasks[Context.Current.ActiveTasks.Count - 1] is BusinessProcess)
                    {
                        foreach (var item in Controls)
                        {
                            var x = item as ENV.UI.TextBox;
                            if (x != null)
                            {
                                if (x.Data.Value is Number)
                                    x.Text = "0";
                            }
                        }
                    }
                });
            }
        }

        protected override Rectangle GetScaledBounds(Rectangle bounds, SizeF factor, BoundsSpecified specified)
        {
            var b = base.GetScaledBounds(bounds, factor, specified);
            if (GetTopLevel())
            {
                // Workaround to WinForms not scaling form location
                if ((specified & BoundsSpecified.X) != BoundsSpecified.None)
                    b.X = (int)Math.Round((double)bounds.X * (double)factor.Width);
                if ((specified & BoundsSpecified.Y) != BoundsSpecified.None)
                    b.Y = (int)Math.Round((double)bounds.Y * (double)factor.Height);
            }
            return b;
        }

        protected FilterBase CndRange(Func<bool> condition, FilterBase filter)
        {
            return FilterBase.CreateConditionedFilter(() => condition(), filter);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Bool> column, Func<bool> fromCondition, Bool fromValue, Func<bool> toCondition, Func<Bool> toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Bool> column, Func<bool> fromCondition, Func<Bool> fromValue, Func<bool> toCondition, Bool toValue)
        {
            return CndRangeBetween(column, fromCondition, fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Date> column, Func<bool> fromCondition, Date fromValue, Func<bool> toCondition, Date toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Number> column, Func<bool> fromCondition, Func<Number> fromValue, Func<bool> toCondition, Number toValue)
        {
            return CndRangeBetween(column, fromCondition, fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Number> column, Func<bool> fromCondition, Number fromValue, Func<bool> toCondition, Number toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Number> column, Func<bool> fromCondition, Firefly.Box.Data.NumberColumn fromValue, Func<bool> toCondition, Number toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Number> column, Func<bool> fromCondition, Number fromValue, Func<bool> toCondition, Firefly.Box.Data.NumberColumn toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Number> column, Func<bool> fromCondition, Firefly.Box.Data.NumberColumn fromValue, Func<bool> toCondition, Firefly.Box.Data.NumberColumn toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Number> column, Func<bool> fromCondition, Number fromValue, Func<bool> toCondition, Func<Number> toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Bool> column, Func<bool> fromCondition, Bool fromValue, Func<bool> toCondition, Bool toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Text> column, Func<bool> fromCondition, Func<Text> fromValue, Func<bool> toCondition, Text toValue)
        {
            return CndRangeBetween(column, fromCondition, fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Text> column, Func<bool> fromCondition, Text fromValue, Func<bool> toCondition, Text toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Text> column, Func<bool> fromCondition, Text fromValue, Func<bool> toCondition, Func<Text> toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, toValue);
        }

        protected FilterBase CndRangeBetween<T>(TypedColumnBase<T> column, Func<bool> fromCondition, Func<T> fromValue, Func<bool> toCondition, Func<T> toValue)
        {
            return new DynamicFilter(where =>
            {
                var fromIsTrue = fromCondition();
                var toIsTrue = toCondition();
                if (fromIsTrue && toIsTrue)
                    where.Add(column.IsBetween(fromValue, toValue));
                else if (fromIsTrue)
                    where.Add(column.IsGreaterOrEqualTo(fromValue));
                else if (toIsTrue)
                    where.Add(column.IsLessOrEqualTo(toValue));
            });
        }
        protected FilterBase CndRangeBetween<T>(TypedColumnBase<T> column, Func<bool> fromCondition, TypedColumnBase<T> fromValue, Func<bool> toCondition, Func<T> toValue)
        {
            return new DynamicFilter(where =>
            {
                var fromIsTrue = fromCondition();
                var toIsTrue = toCondition();
                if (fromIsTrue && toIsTrue)
                    where.Add(column.IsBetween(fromValue, toValue));
                else if (fromIsTrue)
                    where.Add(column.IsGreaterOrEqualTo(fromValue));
                else if (toIsTrue)
                    where.Add(column.IsLessOrEqualTo(toValue));
            });
        }
        protected FilterBase CndRangeBetween<T>(TypedColumnBase<T> column, Func<bool> fromCondition, Func<T> fromValue, Func<bool> toCondition, TypedColumnBase<T> toValue)
        {
            return new DynamicFilter(where =>
            {
                var fromIsTrue = fromCondition();
                var toIsTrue = toCondition();
                if (fromIsTrue && toIsTrue)
                    where.Add(column.IsBetween(fromValue, toValue));
                else if (fromIsTrue)
                    where.Add(column.IsGreaterOrEqualTo(fromValue));
                else if (toIsTrue)
                    where.Add(column.IsLessOrEqualTo(toValue));
            });
        }
        protected FilterBase CndRangeBetween<T>(TypedColumnBase<T> column, Func<bool> fromCondition, TypedColumnBase<T> fromValue, Func<bool> toCondition, TypedColumnBase<T> toValue)
        {
            return new DynamicFilter(where =>
            {
                var fromIsTrue = fromCondition();
                var toIsTrue = toCondition();
                if (fromIsTrue && toIsTrue)
                    where.Add(column.IsBetween(fromValue, toValue));
                else if (fromIsTrue)
                    where.Add(column.IsGreaterOrEqualTo(fromValue));
                else if (toIsTrue)
                    where.Add(column.IsLessOrEqualTo(toValue));
            });
        }
        [Serializable]
        public class FormState
        {
            public System.Drawing.Rectangle Bounds { get; set; }
            public Firefly.Box.UI.WindowStartPosition StartPosition { get; set; }
            public FormWindowState WindowState { get; set; }
            public float SplitterPosition { get; set; }

            public Size SizeInContainer { get; set; }

            public Size ClientSize { get; set; }

            [Serializable]
            public class ColumnState
            {
                public string Name { get; set; }
                public int Width { get; set; }
                public int Index { get; set; }
                public bool UserVisible { get; set; }
                public bool IgnoreWidth;
            }

            [Serializable]
            public class DockedSubFormState
            {
                public string Name { get; set; }
                public Size Size { get; set; }
            }

            public List<ColumnState> Columns = new List<ColumnState>();
            public List<DockedSubFormState> DockedSubForms = new List<DockedSubFormState>();

            public void ApplyOriginalState(Form form)
            {
                var formToApplyBoundsTo = form.ContainerForm ?? form;
                var b = new Rectangle(form.scaleH(Bounds.X), form.scaleV(Bounds.Y), form.scaleH(Bounds.Width), form.scaleV(Bounds.Height));
                var oldSize = formToApplyBoundsTo.Size;
                var oldDock = formToApplyBoundsTo.Dock;
                form.SuspendLayout();
                formToApplyBoundsTo.Dock = DockStyle.None;
                formToApplyBoundsTo.Size = b.Size;
                try
                {
                    ApplyAfterAdvancedLayoutInitialized(form);
                    ApplyAfterLoadingBindingsApplied(form);
                }
                finally
                {
                    form.ResumeLayout(true);
                    if (formToApplyBoundsTo.ChildWindow || oldDock != DockStyle.None)
                    {
                        formToApplyBoundsTo.Size = oldSize;
                        formToApplyBoundsTo.Dock = oldDock;
                    }
                }
            }

            public void ApplyAfterAdvancedLayoutInitialized(Form form)
            {
                var formToApplyBoundsTo = form.ContainerForm ?? form;
                var b = new Rectangle(form.scaleH(Bounds.X), form.scaleV(Bounds.Y), form.scaleH(Bounds.Width), form.scaleV(Bounds.Height));
                if ((form._doNotLoadFormStateBounds & BoundsSpecified.X) == BoundsSpecified.X)
                    b.X = formToApplyBoundsTo.Left;
                if ((form._doNotLoadFormStateBounds & BoundsSpecified.Y) == BoundsSpecified.Y)
                    b.Y = formToApplyBoundsTo.Top;
                if ((form._doNotLoadFormStateBounds & BoundsSpecified.Width) == BoundsSpecified.Width)
                    b.Width = formToApplyBoundsTo.Width;
                if ((form._doNotLoadFormStateBounds & BoundsSpecified.Height) == BoundsSpecified.Height)
                    b.Height = formToApplyBoundsTo.Height;
                if (!form.ChildWindow)
                {
                    if (StartPosition == WindowStartPosition.Custom)
                        formToApplyBoundsTo.Bounds = b;
                    else
                        formToApplyBoundsTo.Size = b.Size;
                    formToApplyBoundsTo.StartPosition = StartPosition;

                    if (formToApplyBoundsTo != form && SizeInContainer != Size.Empty && !form.UseContainerForm)
                        form.ClientSize = new Size(form.scaleH(SizeInContainer.Width), form.scaleV(SizeInContainer.Height));
                }
                if (formToApplyBoundsTo.Parent == null && WindowState != FormWindowState.Minimized)
                    formToApplyBoundsTo.WindowState = WindowState;

                form.SplitterPosition = SplitterPosition;
            }

            public void ApplyAfterLoadingBindingsApplied(Form form)
            {
                foreach (var dockedSubFormState in DockedSubForms)
                {
                    foreach (var control in form.Controls)
                    {
                        var sf = control as SubForm;
                        if (sf != null && sf.Name == dockedSubFormState.Name)
                        {
                            sf.Size = new Size(form.scaleH(dockedSubFormState.Size.Width), form.scaleV(dockedSubFormState.Size.Height));
                            break;
                        }
                    }
                }

                foreach (var control in form.Controls)
                {
                    var g = control as ENV.UI.Grid;
                    if (g != null)
                    {
                        foreach (var columnState in Columns)
                        {
                            foreach (var control1 in g.Controls)
                            {
                                var gc = control1 as ENV.UI.GridColumn;

                                if (gc != null && gc.Name == columnState.Name)
                                {
                                    g.Controls.SetChildIndex(gc, columnState.Index);
                                    if (!columnState.IgnoreWidth)
                                        gc.Width = form.scaleH(columnState.Width);
                                    gc.UserVisible = columnState.UserVisible;
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }
        int unscaleH(int value)
        {
            return (int)Math.Round((float)value / _scalingFactor.Width);
        }
        int unscaleV(int value)
        {
            return (int)Math.Round((float)value / _scalingFactor.Height);
        }
        int scaleH(int value)
        {
            return (int)Math.Round((float)value * _scalingFactor.Width);
        }
        int scaleV(int value)
        {
            return (int)Math.Round((float)value * _scalingFactor.Height);
        }
        FormState CreateFormState(bool original)
        {

            var formToGetBoundsFrom = ContainerForm ?? this;
            var bounds = formToGetBoundsFrom.WindowState != FormWindowState.Normal ? formToGetBoundsFrom.RestoreBounds : formToGetBoundsFrom.Bounds;
            bounds = new Rectangle(unscaleH(bounds.X), unscaleV(bounds.Y), unscaleH(bounds.Width), unscaleV(bounds.Height));
            var result = new FormState() { Bounds = bounds, StartPosition = original ? StartPosition : WindowStartPosition.Custom, WindowState = formToGetBoundsFrom.WindowState, SplitterPosition = SplitterPosition, ClientSize = ClientSize };
            if (ContainerForm != null)
                result.SizeInContainer = new Size(unscaleH(ClientSize.Width), unscaleV(ClientSize.Height));
            ENV.UI.Grid g = null;
            foreach (var control in Controls)
            {
                g = control as ENV.UI.Grid;
                if (g != null)
                {
                    int i = 0;
                    foreach (var c in g.Controls)
                    {
                        var gc = c as ENV.UI.GridColumn;
                        if (gc != null)
                            result.Columns.Add(new FormState.ColumnState { Name = gc.Name, Index = i, Width = unscaleH(gc.Width), UserVisible = gc.UserVisible, IgnoreWidth = gc.WidthChangedDueToBindWidth });
                        i++;
                    }
                    break;
                }

            }

            foreach (var control in Controls)
            {
                var sf = control as SubForm;
                if (sf != null && sf.Dock != DockStyle.None)
                    result.DockedSubForms.Add(new FormState.DockedSubFormState() { Name = sf.Name, Size = new Size(unscaleH(sf.Size.Width), unscaleV(sf.Size.Height)) });
            }

            if (!original && result.ClientSize == _originalState.ClientSize)
                result.Bounds = new Rectangle(result.Bounds.Location, _originalState.Bounds.Size);

            return result;
        }

        FormState _originalState;

        public override void OnBeforeClosing()
        {
            if (_originalState == null || IsDisposed)
            {
                base.OnBeforeClosing();
                return;
            }
            var formState = CreateFormState(false);
            base.OnBeforeClosing();
            SaveUserState(formState);
        }

        protected override void Dispose(bool disposing)
        {
            if (_originalState == null)
            {
                base.Dispose(disposing);
                return;
            }
            var formState = CreateFormState(false);
            base.Dispose(disposing);
            SaveUserState(formState);
        }

        bool _userStateSaved;
        void SaveUserState(FormState createFormState)
        {
            if (_userStateSaved || _originalState == null) return;
            if (!string.IsNullOrEmpty(UserStateIdentifier) && !AutomaticResizeScreens && !ClientSize.IsEmpty)
            {
                try
                {
                    using (var sw = new StreamWriter(FormStateFileName))
                    {
                        var x = new XmlSerializer(typeof(FormState));
                        x.Serialize(sw, createFormState);
                    }
                }
                catch (Exception ex)
                {
                    ErrorLog.WriteToLogFile(ex, "Form state seralization");
                }
            }
            _userStateSaved = true;
        }

        string FormStateFileName
        {
            get
            {
                return GetFormStateFileName(UserStateIdentifier);
            }
        }

        static string GetFormStateFileName(string userStateIdentifier)
        {
            return System.IO.Path.Combine(System.Windows.Forms.Application.UserAppDataPath,
                                          PathDecoder.FixFileName(userStateIdentifier) + ".xml");
        }

        public static bool FormStateClear(Text name)
        {
            var result = false;
            if (string.IsNullOrEmpty(name))
            {
                name = "--";
                var x = Firefly.Box.Context.Current.ActiveTasks;
                if (x.Count > 0)
                {
                    var y = x[x.Count - 1];
                    var f = y.View as ENV.UI.Form;
                    if (f != null)
                    {
                        name = f.UserStateIdentifier;
                    }
                }
            }
            try
            {
                if (string.IsNullOrEmpty(name))
                    return false;
                var fn = GetFormStateFileName(name);
                if (System.IO.File.Exists(fn))
                {
                    System.IO.File.Delete(fn);
                    result = true;
                }
                foreach (var item in Directory.GetFiles(Path.GetDirectoryName(fn), Path.GetFileNameWithoutExtension(fn) + ".*"))
                {
                    System.IO.File.Delete(item);

                }
                if (name == "*")
                {
                    foreach (var item in System.IO.Directory.GetFiles(Application.UserAppDataPath, "*.xml"))
                    {
                        try
                        {
                            File.Delete(item);
                            result = true;
                        }
                        catch { }
                    }
                }
                foreach (var openForm in System.Windows.Forms.Application.OpenForms)
                {
                    var f = openForm as ENV.UI.Form;
                    if (f != null && f._originalState != null)
                    {
                        bool clearState = name == "*";
                        var x = f;
                        while (!clearState && x != null)
                        {
                            if (x.UserStateIdentifier == name)
                                clearState = true;
                            x = x.Parent as ENV.UI.Form;
                        }

                        if (clearState)
                        {
                            Firefly.Box.Context.Current.InvokeUICommand(() =>
                            {
                                if (f._originalState != null)
                                    f._originalState.ApplyOriginalState(f);
                                result = true;
                            });

                        }
                    }
                }


            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "delete form state");
            }
            return result;

        }

        internal static void SetSubFormToAttachLoadedFormTo(SubForm sf)
        {
            _subFormToAttachLoadedFormTo.Value = sf;
        }
        static ContextStatic<SubForm> _subFormToAttachLoadedFormTo = new ContextStatic<SubForm>(() => null);

        bool _isComponent;
        bool _boundExtenders = false;
        class ControlExtenderHelper : ENV.Utilities.IControlHelperControl
        {
            Control _control;
            ColumnBase _column;
            ControlExtender _controlExtender;

            public ControlExtenderHelper(Control control, ColumnBase column, ControlExtender controlExtender)
            {
                _control = control;
                _column = column;
                _controlExtender = controlExtender;
            }

            public bool ReadOnly
            {
                get
                {
                    return false;
                }

                set
                {
                }
            }

            public string ToolTip
            {
                get
                {
                    return "";
                }

                set
                {
                }
            }

            public event Action Expand;
            public event Action Load;

            public ColumnBase GetColumn()
            {
                return _column;
            }

            public Control GetControl()
            {
                return _control;
            }

            public Control GetControlForFocus()
            {
                return _control;
            }
        }
        internal void DoControllerLoad(bool isComponent, ColumnCollection columns)
        {
            if (!_boundExtenders)
            {
                _boundExtenders = true;
                foreach (var e in ControlExtenders())
                {
                    foreach (var c in columns)
                    {
                        if (c.Value == e.Control)
                        {
                            var ch = new ControlHelper(new ControlExtenderHelper(e.Control, c, e));
                            e.Enter += () => ch.ControlEnter(() => { });
                            e.Leave += () => ch.ControlLeave(() => { });
                            e.InputValidation += () => ch.ControlInputValidation(() => { });
                        }

                    }

                }

            }

            if (BindUserStateIdentifier != null)
            {
                var x = new StringBindingEventArgs(UserStateIdentifier);
                BindUserStateIdentifier(this, x);
                UserStateIdentifier = x.Value;
            }
            if (!base.DesignMode && string.IsNullOrEmpty(UserStateIdentifier) && AutoUserStateIdentifier)
                UserStateIdentifier = this.GetType().FullName;
            if (!string.IsNullOrEmpty(UserStateIdentifier))
            {
                if (u.SubformExecMode(0) >= 0)
                {
                    var x = Firefly.Box.Context.Current.ActiveTasks;
                    if (x.Count > 1)
                    {
                        var y = x[x.Count - 2];
                        if (y is ModuleController && x.Count > 2)
                            y = x[x.Count - 3];
                        var f = y.View as ENV.UI.Form;
                        if (f != null)
                        {
                            UserStateIdentifier = !string.IsNullOrEmpty(f.UserStateIdentifier) ? f.UserStateIdentifier + ".sf." + UserStateIdentifier : AutoUserStateIdentifier ? "" : UserStateIdentifier;
                            if (UserStateIdentifier.Length > 128)
                            {
                                UserStateIdentifier = UserStateIdentifier.Remove(128) + UserStateIdentifier.GetHashCode().ToString();
                            }
                        }
                    }
                }
            }
            foreach (var item in Controls)
            {
                var x = item as ENV.UI.SubForm;
                if (x != null)
                {
                    x.DoControllerLoadForFrame(isComponent, columns);
                }
            }
            _isComponent = isComponent;
            OnControllerLoad();
            if (UseContainerForm)
                Firefly.Box.Context.Current.InvokeUICommand(() => ContainerForm = new Firefly.Box.UI.Form());
            Firefly.Box.Context.Current.InvokeUICommand(
                () =>
                {
                    var x = _subFormToAttachLoadedFormTo.Value;
                    if (x != null)
                    {
                        _subFormToAttachLoadedFormTo.Value = null;
                        AttachToSubForm(x);
                    }
                    if (TopLevel && Owner != null)
                    {
                        ControlBox = false;
                        MinimizeBox = false;
                        MaximizeBox = false;
                    }
                });
            if (GetForceTopLevel() && ContainerForm != null)
                Common.SetContextTopMostFormIfNull(ContainerForm);

            if (_backwardCompatibleZOrder.Count > 0)
            {
                var l = new List<Control>();
                ForEachControlInZOrder(c => l.Add(c));
                foreach (var item in _backwardCompatibleZOrder)
                    l.Insert(l.IndexOf(item.Value) + 1, item.Key);
                var i = 1;
                foreach (var c in l)
                {
                    var cb = c as ControlBase;
                    if (cb != null)
                        cb.ZOrder = i++;
                }
            }
        }
        public bool UseContainerForm { get; set; }
        protected virtual void OnControllerLoad()
        {


        }
        protected void SetContainerForm(Func<Firefly.Box.UI.Form> sdiFactory)
        {
            SetContainerForm(sdiFactory, -1, true);
        }
        protected void SetContainerForm(Func<Firefly.Box.UI.Form> sdiFactory, bool showMenu = true)
        {
            SetContainerForm(sdiFactory, -1, showMenu);
        }

        protected void SetContainerForm(Func<Firefly.Box.UI.Form> sdiFactory, int menuToShow = -1, bool showMenu = true)
        {
            if (!showMenu)
                menuToShow = -1;
            if (u.SubformExecMode(0) >= 0) return;

            if (UserSettings.VersionXpaCompatible || (!ENV.Common.IsRootMdiContext || ApplicationControllerBase._noMdi) && !Common.ContextTopMostFormWasSet)
                Firefly.Box.Context.Current.InvokeUICommand(
                    () =>
                    {
                        var f = sdiFactory();
                        f.ShowInTaskbar = true;
                        {
                            var ef = f as ENV.UI.Form;
                            if (ef != null)
                                ef.AutoUserStateIdentifier = false;
                        }
                        ContainerForm = f;

                        if (!SystemMenu && ContainerForm != null)
                        {
                            foreach (var c in ContainerForm.Controls)
                            {
                                var tsi = c as ToolStrip;
                                if (tsi != null && !(tsi is StatusStrip))
                                    tsi.Visible = false;
                            }
                        }
                        var z = f as IHaveAMenu;
                        if (z != null && menuToShow > 0)
                            z.DoOnMenu(m => m.Activate(menuToShow, showMenu, ""));
                    });
        }

        protected override void OnAdvancedAnchorLayoutInitialized()
        {
            base.OnAdvancedAnchorLayoutInitialized();
            if (!string.IsNullOrEmpty(UserStateIdentifier) && !AutomaticResizeScreens)
            {
                _originalState = CreateFormState(true);
                try
                {
                    if (System.IO.File.Exists(FormStateFileName))
                        using (var sr = new StreamReader(FormStateFileName))
                        {
                            var x = new XmlSerializer(typeof(FormState));
                            var state = x.Deserialize(sr) as FormState;
                            if (state != null)
                            {
                                var cw = new BooleanBindingEventArgs(ChildWindow);
                                if (_bindChildWindow != null)
                                    RunInLogicContext(() => _bindChildWindow(this, cw));
                                ChildWindow = cw.Value;
                                state.ApplyAfterAdvancedLayoutInitialized(this);
                                LoadingBindingsApplied += () => state.ApplyAfterLoadingBindingsApplied(this);
                            }
                        }
                }
                catch (Exception ex)
                {
                    ErrorLog.WriteToLogFile(ex, "Form state read");
                }
            }

            if (Modal && StartPosition == WindowStartPosition.Default)
                StartPosition = WindowStartPosition.Custom;


        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (_inAFrame)
            {
                _frameContainerForm = Parent as ENV.UI.Form;
            }
            if (IsMdiChild && AnchorByOriginalMdiParentSize)
            {
                var mdiSize = Parent.Size;
                Size = new Size(Size.Width - OriginalMdiParentSize.Width + mdiSize.Width,
                    Size.Height - OriginalMdiParentSize.Height + mdiSize.Height);
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            }
        }

        protected bool AnchorByOriginalMdiParentSize;
        protected Size OriginalMdiParentSize = new Size(800, 600);
        public bool UseSystemControlBackColorForTransparentBackColor { get; set; }
        bool _inAFrame = false;
        ENV.UI.Form _frameContainerForm;
        public void YouAreInAFrame()
        {
            _inAFrame = true;
        }
        protected override void OnLoad(EventArgs e)
        {

            if (FitToMDI && !TopLevel && !ChildWindow)
            {
                if (UserSettings.VersionXpaCompatible && !TitleBar)
                    Border = ControlBorderStyle.Thin;
                else
                    Border = ControlBorderStyle.None;
                MinimizeBox = false;
                MaximizeBox = false;
            }

            if (!UserSettings.VersionXpaCompatible && TopLevel && !ForceTopLevel && Owner != null)
            {
                MinimizeBox = false;
                MaximizeBox = false;
            }

            base.OnLoad(e);
            FaceLiftDemo.MatchBackColor(this);
            if (!ChildWindow && UseSystemControlBackColorForTransparentBackColor && ColorScheme != null && ColorScheme.TransparentBackground)
                BackColor = SystemColors.Control;

            if (BindShowInWindowsMenu != null)
            {
                var args = new BooleanBindingEventArgs(ShowInWindowsMenu);
                BindShowInWindowsMenu(this, args);
                ShowInWindowsMenu = args.Value;
            }
            if (!ChildWindow && ShowInWindowsMenu)
                if (true)
                {
                    lock (_windowList) _windowList.Add(this);
                    FormClosed += delegate { lock (_windowList) _windowList.Remove(this); };
                }
        }
        internal static ENV.UI.Form _activeForm;
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            _activeForm = this;
        }

        public bool ShowInWindowsMenu { get; set; }
        public event BindingEventHandler<BooleanBindingEventArgs> BindShowInWindowsMenu;

        internal static List<ENV.UI.Form> _windowList = new List<Form>();
        public CustomHelp CustomHelp { get; set; }

        System.Windows.Forms.Control _focusedControl;
        public new System.Windows.Forms.Control FocusedControl { get { return _focusedControl; } }
        internal void SetFocusedControl(System.Windows.Forms.Control control)
        {
            _focusedControl = control;
            if (_frameContainerForm != null)
                _frameContainerForm.SetFocusedControl(control);
        }

        public void Run()
        {
            var uic = new UIControllerBase() { View = () => this };
            uic.Execute();
        }

        protected override ContextMenuStrip GetContainerContextMenuStrip()
        {
            return !_isComponent && Common.ContextTopMostForm != null ? Common.ContextTopMostForm.ContextMenuStrip : null;
        }
        event BindingEventHandler<StringBindingEventArgs> _savedbindText;
        protected override void HelpRequestFailed(Exception exception)
        {
            Message.ShowWarningInStatusBar(string.Format(LocalizationInfo.Current.HelpRequestFailed, ApplicationControllerBase.DefaultHelpFile));
        }

        public override event BindingEventHandler<StringBindingEventArgs> BindText
        {
            add
            {
                base.BindText += value;
                _savedbindText += value;
            }
            remove
            {
                base.BindText -= value;
                _savedbindText -= value;
            }
        }
        internal string GetBindTextValue()
        {
            if (_savedbindText == null)
                return null;
            var x = new StringBindingEventArgs(null);
            _savedbindText(this, x);
            return x.Value;
        }
        internal void ControllerEnterRow()
        {
            OnControllerEnterRow();
        }
        protected virtual void OnControllerEnterRow()
        {
        }

        INullStrategy _nullStrategy = NullStrategy.GetStrategy(false);
        internal void SetNullStrategy(INullStrategy instance)
        {
            _nullStrategy = instance;
            if (_uInstance != null)
                _nullStrategy.ApplyTo(_uInstance);
        }
        UserMethods _uInstance;
        protected UserMethods u
        {
            get
            {
                if (_uInstance == null)
                {
                    _uInstance = new UserMethods();
                    _nullStrategy.ApplyTo(_uInstance);
                }
                return _uInstance;
            }
        }

        BoundsSpecified _doNotLoadFormStateBounds = BoundsSpecified.None;
        public override event BindingEventHandler<IntBindingEventArgs> BindClientHeight
        {
            add { base.BindClientHeight += value; _doNotLoadFormStateBounds |= BoundsSpecified.Height; }
            remove { base.BindClientHeight -= value; }
        }
        public override event BindingEventHandler<IntBindingEventArgs> BindClientWidth
        {
            add { base.BindClientWidth += value; _doNotLoadFormStateBounds |= BoundsSpecified.Width; }
            remove { base.BindClientWidth -= value; }
        }
        public override event BindingEventHandler<IntBindingEventArgs> BindLeft
        {
            add { base.BindLeft += value; _doNotLoadFormStateBounds |= BoundsSpecified.X; }
            remove { base.BindLeft -= value; }
        }
        public override event BindingEventHandler<IntBindingEventArgs> BindTop
        {
            add { base.BindTop += value; _doNotLoadFormStateBounds |= BoundsSpecified.Y; }
            remove { base.BindTop -= value; }
        }

        protected override void OnContextMenuChanged(EventArgs e)
        {

            base.OnContextMenuChanged(e);
        }

        internal bool GetForceTopLevel()
        {
            var x = new BooleanBindingEventArgs(ForceTopLevel);
            if (_bindForceTopLevel != null)
                _bindForceTopLevel(this, x);
            return x.Value;
        }

        event BindingEventHandler<BooleanBindingEventArgs> _bindForceTopLevel;
        public new event BindingEventHandler<BooleanBindingEventArgs> BindForceTopLevel
        {
            add
            {
                base.BindForceTopLevel += value;
                _bindForceTopLevel += value;
            }
            remove
            {
                base.BindForceTopLevel -= value;
                _bindForceTopLevel -= value;
            }
        }

        internal bool BlockFormMovingAndResizing { get; set; }



        Type ICanShowCustomHelp.ExpandClassType { get; set; }
        protected FormWindowState TranslateWindowState(Number stateExpressionResult)
        {
            try
            {
                switch ((int)stateExpressionResult)
                {
                    case 3:
                        return (FormWindowState.Minimized);
                    case 2:
                        return (FormWindowState.Maximized);
                    case 1:
                    default:
                        return (FormWindowState.Normal);
                }
            }
            catch
            {
                return FormWindowState.Normal;
            }
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if (BlockFormMovingAndResizing && m.Msg == 0x0112 /* WM_SYSCOMMAND */)
            {
                var command = m.WParam.ToInt32() & 0xfff0;
                if (command == 0xF010 /* SC_MOVE */ || command == 0xF000 /* SC_SIZE */)
                    return;
            }
            base.WndProc(ref m);
        }
        protected void Try(Action operationToRun)
        {
            Common.Try(operationToRun);
        }

        protected T Create<T>()
        {
            return AbstractFactory.Create<T>();
        }

        Dictionary<ControlBase, ControlBase> _backwardCompatibleZOrder = new Dictionary<ControlBase, ControlBase>();
        internal protected void AddBackwardCompatibleZOrder(ControlBase zOrderThisControl, ControlBase immediatelyAboveThisOne)
        {
            _backwardCompatibleZOrder.Add(zOrderThisControl, immediatelyAboveThisOne);
        }
        ControlsAndVariablesInForm _variablesInForm;
        internal ControlsAndVariablesInForm ControlsAndVariablesInFormHelper()
        {
            if (_variablesInForm == null)
            {
                _variablesInForm = new UI.Form.ControlsAndVariablesInForm();

                ForEachControlInTabOrder(control =>
                {

                    _variablesInForm.Add(control);
                });

            }
            return _variablesInForm;
        }
        internal class ControlsAndVariablesInForm
        {

            Dictionary<ColumnBase, int> _columnIndexes = new Dictionary<ColumnBase, int>();
            Dictionary<int, Control> _controls = new Dictionary<int, Control>();
            internal void Add(Control control)
            {
                int i = _controls.Count + 1;
                _controls.Add(i, control);
                var ic = control as InputControlBase;
                if (ic != null)
                {
                    var col = ENV.Common.GetColumn(ic);
                    if (col != null)
                    {
                        if (!_columnIndexes.ContainsKey(col))
                            _columnIndexes.Add(col, i);
                    }
                }
            }
            public int Count { get { return _controls.Count; } }

            internal int IndexOf(ColumnBase c)
            {
                int result;
                if (_columnIndexes.TryGetValue(c, out result))
                    return result;
                return -1;
            }

            internal Control GetControlAt(Number controlId)
            {
                Control result;
                if (_controls.TryGetValue(controlId, out result))
                    return result;
                return null;
            }
        }
        public override Type GetControlTypeForWizard(ColumnBase column)
        {
            var col = column as IENVColumn;
            if (col != null && col.ControlType != null)
                return col.ControlType;
            return base.GetControlTypeForWizard(column);
        }
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (Modal && StartPosition != WindowStartPosition.Custom)
            {
                var b = Screen.GetBounds(new Point(x, y));
                if (x < b.X)
                    x = b.X;
                if (y < b.Y)
                    y = b.Y;
            }
            base.SetBoundsCore(x, y, width, height, specified);
        }

        [Browsable(false)]
        public bool OnBindFormSetClientSizeBeforeAdvancedAnchorLayout { get; set; }

        event BindingEventHandler<BooleanBindingEventArgs> _bindChildWindow;
        public override event BindingEventHandler<BooleanBindingEventArgs> BindChildWindow
        {
            add { base.BindChildWindow += value; _bindChildWindow += value; }
            remove { base.BindChildWindow -= value; _bindChildWindow -= value; }
        }

        [Browsable(false)]
        internal bool _TitleBarBound;

        public override event BindingEventHandler<BooleanBindingEventArgs> BindTitleBar
        {
            add { base.BindTitleBar += value; _TitleBarBound = true; }
            remove { base.BindTitleBar -= value; }
        }
    }

}
