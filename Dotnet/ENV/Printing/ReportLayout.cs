using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using Firefly.Box;
using Firefly.Box.Advanced;

namespace ENV.Printing
{
    public class ReportLayout:Firefly.Box.Printing.ReportLayout
    {
        
        public ReportLayout()
        {
            SectionType =  typeof (ReportSection);
            UseScaleConversion = true;
        }

        public ReportLayout(AbstractUIController controller) : this()
        {
            Controller = controller._uiController;
            u = controller.u;
        }
        public ReportLayout(BusinessProcessBase controller): this()
        {
            Controller = controller._businessProcess;
            u = controller.u;
        }
        public ReportLayout(BusinessProcess controller,UserMethods u)
            : this()
        {
            Controller = controller;
            this.u = u;
        }
        public ReportLayout(ApplicationControllerBase task)
            : this()
        {
            Controller = task._moduleController;
            u = task.u;
        }

        protected override bool FilterTypesForChangeControlTypes(Type controlType)
        {
            if (controlType.Assembly == typeof(UI.TextBox).Assembly)
                return false;
            var bt = controlType.BaseType;
            while (bt != null)
            {
                if (bt.Namespace.StartsWith(typeof(Printing.TextBox).Namespace))
                    return true;
                bt = bt.BaseType;
            }
            return false;
        }

        internal void SetScaleTo(PrinterWriter p)
        {
            if (UseScaleConversion)
                p.SetScaleUnits(_verticalExpressionFactor / Math.Max(_verticalScale, 1));
            else
                p.SetScaleUnits(1);
        }
        double _horizontalScale = 1, _verticalScale = 1, _horizontalExpressionFactor = 1, _verticalExpressionFactor = 1;
        public bool UseScaleConversion { get; set; }
        [DefaultValue(1)]
        public virtual double HorizontalScale
        {
            get { return _horizontalScale; }
            set { _horizontalScale = value; }
        }
        [DefaultValue(1)]
        public virtual double VerticalScale
        {
            get { return _verticalScale; }
            set { _verticalScale = value; }
        }
        [DefaultValue(1)]
        public virtual double HorizontalExpressionFactor
        {
            get { return _horizontalExpressionFactor; }
            set { _horizontalExpressionFactor = value; }
        }
        [DefaultValue(1)]
        public virtual double VerticalExpressionFactor
        {
            get { return _verticalExpressionFactor; }
            set { _verticalExpressionFactor = value; }
        }
        public int ToPixelHorizontal(Number value)
        {
            return ENV.UI.Form.ToPixel(value, HorizontalScale,HorizontalExpressionFactor);
        }
        public int ToPixelVertical(Number value)
        {
            return ENV.UI.Form.ToPixel(value, VerticalScale,VerticalExpressionFactor);
        }
        List<ReportLayout> _layouts = new List<ReportLayout>();
        internal ITask Controller;
        internal protected UserMethods u;

        public List<ReportLayout> LayoutsWithAdditionalPageHeaders
        {
            get { return _layouts; }
        }
        internal void NewPageStartedBecauseOf(PrinterWriter writer, ReportSection cause)
        {
            foreach (var layout in _layouts)
            {
                layout.NewPageStarted(writer);
                
            }
            
            foreach (var o in Controls)
            {
                var x = o as ReportSection;
                if (x != null)
                {
                    if (x == cause)
                        return;
                    x.NewPageStarted(writer);
                }

            }


        }

        internal void NewPageStarted(PrinterWriter writer)
        {
            foreach (var control in Controls)
            {
                var x = control as ReportSection;
                if (x != null)
                    x.NewPageStarted(writer);
            }
        }

    }
}
