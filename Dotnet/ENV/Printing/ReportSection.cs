using System;
using System.Collections.Generic;
using System.Text;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;
using Firefly.Box.UI;

namespace ENV.Printing
{
    public class ReportSection : Firefly.Box.Printing.ReportSection
    {
        ITask _task;
        internal UserMethods u;
        public ReportSection()
        {
            DefaultLabelType = typeof(ENV.Printing.Label);
            DefaultTextBoxType = typeof(ENV.Printing.TextBox);
        }
        public ReportSection(AbstractUIController task) : this()
        {
            _task = task._uiController;
            u = task.u;
        }
        public ReportSection(BusinessProcessBase task)
            : this()
        {
            _task = task._businessProcess;
            u = task.u;
        }
        public ReportSection(BusinessProcess task, UserMethods u)
            : this()
        {
            _task = task;
            this.u = u;
        }
        public ReportSection(ApplicationControllerBase task)
            : this()
        {
            _task = task._moduleController;
            u = task.u;
        }

        public static event Action BeforeWrite;

        public void WriteTo(PrinterWriter p)
        {
            try
            {
                if (p.DoNotPrint())
                    return;


                ReportLayout parentForm = Parent as ReportLayout;
                Action after = delegate { };
                if (parentForm != null)
                {
                    parentForm.SetScaleTo(p);

                }
                after = p.SetCurrentLayout(parentForm, this);

                WriteTo((Firefly.Box.Printing.PrinterWriter)p);
                after();
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex);
            }
        }
        BusinessProcessBase _lastControllerWhenPrintingGrid;
        long _lastControllerNumOfExecutionWhenPrintingGrid;
        protected override void PrintingWithGrid(Firefly.Box.UI.Grid g)
        {
            var t = ENV.UserMethods.Instance.GetTaskByGeneration(0);
            ControllerBase.SendInstanceBasedOnTaskAndCallStack(t, c =>
            {
                var b = c as BusinessProcessBase;
                if (b != null)
                {
                    if (_lastControllerWhenPrintingGrid != b || _lastControllerNumOfExecutionWhenPrintingGrid != b._numOfExecutions)
                        g.RestartGridPrinting();
                    _lastControllerWhenPrintingGrid = b;
                    _lastControllerNumOfExecutionWhenPrintingGrid = b._numOfExecutions;
                }
            });
        }
        public override Type GetControlTypeForWizard(ColumnBase column)
        {
            var col = column as IENVColumn;
            if (col != null && col.ControlTypePrinting != null)
                return col.ControlTypePrinting;
            return base.GetControlTypeForWizard(column);
        }
        public bool PageHeader { set; get; }
        internal void NewPageStarted(PrinterWriter writer)
        {
            if (PageHeader)
                WriteTo(writer);
        }

        public override void WriteTo(Firefly.Box.Printing.PrinterWriter writer)
        {
            var x = UserMethods.CurrentIO.Value;
            UserMethods.CurrentIO.Value = writer as ENV.Printing.PrinterWriter;
            try
            {
                using (ENV.Utilities.Profiler.StartContext("Write Report Section: " + Name))
                {
                    try
                    {
                        if (_task == null)
                        {
                            ReportLayout parentForm = Parent as ReportLayout;
                            if (parentForm != null)
                            {
                                _task = parentForm.Controller;
                                u = parentForm.u;
                            }
                        }
                        Action after = delegate { };
                        if (u != null)
                            after = u.SetContext(_task);
                        if (BeforeWrite != null)
                            BeforeWrite();
                        try
                        {
                            base.WriteTo(writer);
                        }
                        catch (System.Drawing.Printing.InvalidPrinterException ex)
                        {
                            Common.ShowExceptionDialog(ex, true, "Invalid Printer");
                            writer.CancelPrinting = true;
                        }
                        after();
                    }
                    catch (Exception ex)
                    {
                        ErrorLog.WriteToLogFile(ex);
                    }
                }
            }
            finally {
                UserMethods.CurrentIO.Value = x;
            }
        }

        double _additionalHeight;
        public double AdditionalHeight
        {
            get { return _additionalHeight; }
            set { _additionalHeight = value; }
        }

        protected override double GetExactHeight()
        {
            return Height + _additionalHeight;
        }

        List<Action> _suspendedControls = new List<Action>();
        public void SuspendDataControls()
        {

            _suspendedControls.Clear();
            ForeachControl(Controls, c =>
            {

                if (c is Firefly.Box.UI.Advanced.InputControlBase || c is Firefly.Box.UI.Line)
                {
                    var z = (Firefly.Box.UI.Advanced.ControlBase)c;
                    var x = z.Visible;
                    z.Visible = false;
                    _suspendedControls.Add(() => z.Visible = x);
                }

            });
        }
        public void ResumeDataControls()
        {
            foreach (var item in _suspendedControls)
            {
                item();
            }
            _suspendedControls.Clear();
        }

        void ForeachControl(ControlCollection col, Action<System.Windows.Forms.Control> on)
        {
            foreach (var item in col)
            {
                var c = item as System.Windows.Forms.Control;
                if (c != null)
                {
                    on(c);
                    ForeachControl(c.Controls, on);
                }
            }
        }
        public override FontScheme FontScheme
        {
            get
            {
                return base.FontScheme;
            }

            set
            {
                base.FontScheme = ENV.UI.LoadableFontScheme.GetPrintingFont(value);
            }
        }
    }
}
