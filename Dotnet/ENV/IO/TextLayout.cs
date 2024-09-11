using System;
using System.Collections.Generic;
using System.Drawing;
using ENV.IO.Advanced;
using ENV.IO.Advanced.Internal;
using ENV.Printing;
using Firefly.Box.Advanced;
using Firefly.Box;
using System.ComponentModel;
using System.Collections;
using System.Windows.Forms;

namespace ENV.IO
{
    public class TextLayout : ENV.Printing.ReportLayout
    {

        public TextLayout()
        {
            SectionType = typeof(TextSection);
            UseScaleConversion = true;
            HorizontalScale = 9.6D;
            VerticalScale = 17.11D;
            ExtraHeightAtBottom =(int) VerticalScale;
            _fontScheme = new Firefly.Box.UI.FontScheme { Font = new Font("Courier New", 12) };
        
        }
        protected override void OnControlAdded(ControlEventArgs e)
        {
            var c = e.Control as TextSection;
            if (c != null)
            {
                c.Font = Font;
                var x = c.HeightInChars;
                var y = c.WidthInChars;
                
                c.HorizontalScale = HorizontalScale;
                c.VerticalScale = VerticalScale;

                c.HeightInChars = x;
                c.WidthInChars = y;
                foreach (var item in c.Controls)
                {
                    var tb = item as ENV.IO.Advanced.TextControlBase;
                    if (tb != null)
                    {
                        var a = tb.SizeInChars;
                        var b = tb.LocationInChars;
                        tb.HorizontalScale = HorizontalScale;
                        tb.VerticalScale = VerticalScale;
                        tb.SizeInChars = a;
                        tb.LocationInChars = b;

                    }
                }

            }
            base.OnControlAdded(e);
        }

        protected override bool FilterTypesForChangeControlTypes(Type controlType)
        {
            if (controlType.Assembly == typeof(UI.TextBox).Assembly)
                return false;
            var bt = controlType.BaseType;
            while (bt != null)
            {
                if (bt.Namespace.StartsWith(typeof(IO.TextBox).Namespace))
                    return true;
                bt = bt.BaseType;
            }
            return false;
        }
        class myForm : Firefly.Box.UI.Form
        {
            public SizeF GetFontSize(System.Drawing.Font font)
            {
                return GetAverageCharSize(font);
            }
        }
        Firefly.Box.UI.FontScheme _fontScheme;
        public  Firefly.Box.UI.FontScheme FontScheme
        {
            get
            {
                return _fontScheme;
            }

            set
            {
                if (Font != value.Font)
                {
                    Font = value.Font;
                    using (var f = new myForm())
                    {
                        var x = f.GetFontSize(Font);
                        var a = WidthInChars;

                        HorizontalScale = x.Width;
                        VerticalScale = x.Height;

                        WidthInChars = a;
                    }
                }
                _fontScheme = value;
            }
        }
        public TextLayout(AbstractUIController controller) : this()
        {
            Controller = controller._uiController;
            u = controller.u;
        }
        public TextLayout(BusinessProcessBase controller) : this()
        {
            Controller = controller._businessProcess;
            u = controller.u;
        }
        public TextLayout(BusinessProcess controller, UserMethods u)
            : this()
        {
            Controller = controller;
            this.u = u;
        }
        public TextLayout(ApplicationControllerBase task)
            : this()
        {
            Controller = task._moduleController;
            u = task.u;
        }

        List<TextLayout> _layouts = new List<TextLayout>();

        
        protected override bool IsControlTypeSupported(Type controlType)
        {
            return typeof(ENV.IO.Advanced.TextControlBase).IsAssignableFrom(controlType);
        }
        public new List<TextLayout> LayoutsWithAdditionalPageHeaders
        {
            get { return _layouts; }
        }
        [System.ComponentModel.DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override double HorizontalExpressionFactor
        {
            get
            {
                return base.HorizontalExpressionFactor;
            }

            set
            {
                base.HorizontalExpressionFactor = value;
            }
        }
        [System.ComponentModel.DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override double HorizontalScale
        {
            get
            {
                return base.HorizontalScale;
            }

            set
            {
                base.HorizontalScale = value;
            }
        }
        [System.ComponentModel.DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override double VerticalExpressionFactor
        {
            get
            {
                return base.VerticalExpressionFactor;
            }

            set
            {
                base.VerticalExpressionFactor = value;
            }
        }
        [System.ComponentModel.DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override double VerticalScale
        {
            get
            {
                return base.VerticalScale;
            }

            set
            {
                base.VerticalScale = value;
            }
        }
        [System.ComponentModel.DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override int Width
        {
            get
            {
                return base.Width;
            }

            set
            {
                base.Width = value;
            }
        }
        protected override void PostFilterProperties(IDictionary properties)
        {
            base.PostFilterProperties(properties);
            return;
            /*System.Diagnostics.Debug.WriteLine("Properties:");
            foreach (string item in properties.Keys)
            {
                System.Diagnostics.Debug.WriteLine("\"" + item + "\"");
            }*/
            foreach (var item in new string[] {
                "Layouts"
,"HorizontalExpressionFactor"
,"HorizontalScale"
,"VerticalExpressionFactor"
,"VerticalScale"
,"Width"
//,"UseScaleConversion"
//,"Name"
//,"Size"
,"Padding"

,"AutoSize"
,"AutoSizeMode"
,"AutoValidate"
,"BorderStyle"
,"Text"
,"AutoScaleDimensions"
,"AutoScaleMode"
,"BindingContext"
,"ActiveControl"
,"CurrentAutoScaleDimensions"
,"ParentForm"
,"AutoScroll"
,"AutoScrollMargin"
,"AutoScrollPosition"
,"AutoScrollMinSize"
,"DisplayRectangle"
,"HorizontalScroll"
,"VerticalScroll"
,"DockPadding"
,"AccessibilityObject"
,"AccessibleDefaultActionDescription"
,"AccessibleDescription"
,"AccessibleName"
,"AccessibleRole"
,"AllowDrop"
,"Anchor"
,"AutoScrollOffset"
,"LayoutEngine"
,"BackColor"
,"BackgroundImage"
,"BackgroundImageLayout"
,"Bottom"
,"Bounds"
,"CanFocus"
,"CanSelect"
,"Capture"
,"CausesValidation"
,"ClientRectangle"
,"ClientSize"
,"CompanyName"
,"ContainsFocus"
,"ContextMenu"
,"ContextMenuStrip"
,"Controls"
,"Created"
,"Cursor"
,"DataBindings"
,"IsDisposed"
,"Disposing"
,"Dock"
,"Enabled"
,"Focused"
,"Font"
,"ForeColor"
,"Handle"
,"HasChildren"
,"Height"
,"IsHandleCreated"
,"InvokeRequired"
,"IsAccessible"
,"IsMirrored"
,"Left"
,"Location"
,"Margin"
,"MaximumSize"
,"MinimumSize"
,"Parent"
,"ProductName"
,"ProductVersion"
,"RecreatingHandle"
,"Region"
,"Right"
,"RightToLeft"
,"Site"
,"TabIndex"
,"TabStop"
,"Tag"
,"Top"
,"TopLevelControl"
,"UseWaitCursor"
,"Visible"
,"WindowTarget"
,"PreferredSize"
,"ImeMode"
,"Container"
,"DefaultModifiers"
,"LoadLanguage"
,"Language"
,"Localizable"
,"ApplicationSettings"
,"Locked"
,"CurrentGridSize"
,"TrayHeight"
,"TrayLargeIcon"
,"DoubleBuffered"

            })
            {
                if (properties.Contains(item))
                    properties.Remove(item);
            }
        }
        protected override void PostFilterEvents(IDictionary events)
        {
            base.PostFilterEvents(events);
            return;
            foreach (var item in new string[] {
                "AutoSizeChanged"
,"AutoValidateChanged"
,"Load"
,"TextChanged"
,"Scroll"
,"BackColorChanged"
,"BackgroundImageChanged"
,"BackgroundImageLayoutChanged"
,"BindingContextChanged"
,"CausesValidationChanged"
,"ClientSizeChanged"
,"ContextMenuChanged"
,"ContextMenuStripChanged"
,"CursorChanged"
,"DockChanged"
,"EnabledChanged"
,"FontChanged"
,"ForeColorChanged"
,"LocationChanged"
,"MarginChanged"
,"RegionChanged"
,"RightToLeftChanged"
,"SizeChanged"
,"TabIndexChanged"
,"TabStopChanged"
,"VisibleChanged"
,"Click"
,"ControlAdded"
,"ControlRemoved"
,"DragDrop"
,"DragEnter"
,"DragOver"
,"DragLeave"
,"GiveFeedback"
,"HandleCreated"
,"HandleDestroyed"
,"HelpRequested"
,"Invalidated"
,"PaddingChanged"
,"Paint"
,"QueryContinueDrag"
,"QueryAccessibilityHelp"
,"DoubleClick"
,"Enter"
,"GotFocus"
,"KeyDown"
,"KeyPress"
,"KeyUp"
,"Layout"
,"Leave"
,"LostFocus"
,"MouseClick"
,"MouseDoubleClick"
,"MouseCaptureChanged"
,"MouseDown"
,"MouseEnter"
,"MouseLeave"
,"MouseHover"
,"MouseMove"
,"MouseUp"
,"MouseWheel"
,"Move"
,"PreviewKeyDown"
,"Resize"
,"ChangeUICues"
,"StyleChanged"
,"SystemColorsChanged"
,"Validating"
,"Validated"
,"ParentChanged"
,"ImeModeChanged"
,"Disposed"


            })
            {
                if (events.Contains(item))
                    events.Remove(item);
            }
         /*   System.Diagnostics.Debug.WriteLine("Events:");
            foreach (string item in events.Keys)
            {
                System.Diagnostics.Debug.WriteLine("\"" + item + "\"");
            }*/
        }


        internal void NewPageStarted(SectionWriter writer)
        {
            foreach (var control in Controls)
            {
                var x = control as TextSection;
                if (x != null)
                    x.NewPageStarted(writer);
            }
        }
        internal void NewPageStartedBecauseOf(SectionWriter writer, ReportSection cause)
        {
            foreach (var layout in _layouts)
            {
                layout.NewPageStarted(writer);

            }

            foreach (var o in Controls)
            {
                var x = o as TextSection;
                if (x != null)
                {
                    if (x == cause)
                        return;
                    x.NewPageStarted(writer);
                }

            }

        }
        public int WidthInChars
        {
            set { Width  =ToPixel( new Size(value, ToChars(Size).Height)).Width; }
            get { return ToChars(Size).Width; }

        }

        
      
        private Point ToCharsWithMovement(Point p)
        {
            var x = ToChars(p);
            var xp = ToPixel(x);
            if (p.X == xp.X + 1)
            {
                x.X += 1;
            }
            else if (p.X == xp.X - 1)
            {
                x.X -= 1;
            }
            if (p.Y == xp.Y + 1)
            {
                x.Y += 1;
            }
            else if (p.Y == xp.Y - 1)
                x.Y -= 1;
            return x;
        }
        Point ToPoint(Size s)
        {
            return new Point(s.Width, s.Height);
        }
        Size ToSize(Point s)
        {
            return new Size(s.X, s.Y);
        }
        Point ToChars(Point p)
        {
            return new Point((int)Math.Round(p.X / HorizontalScale),
                    (int)Math.Round(p.Y / VerticalScale));
        }
        Size ToChars(Size s)
        {
            return ToSize(ToChars(ToPoint(s)));
        }
        Point ToPixel(Point value)
        {
            return new Point((int)(value.X * HorizontalScale),
                 (int)(value.Y * VerticalScale));
        }
        Size ToPixel(Size s)
        {
            return ToSize(ToPixel(ToPoint(s)));
        }
    }
}
