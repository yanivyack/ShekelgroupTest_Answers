using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firefly.Box.Data.Advanced;
using Firefly.Box.UI;
using Firefly.Box.UI.Advanced;

namespace ENV.UI
{
    public class SubForm : Firefly.Box.UI.SubForm
    {
        public SubForm()
        {

        }
        protected override string Translate(string term)
        {
            return base.Translate(ENV.Languages.Translate(term));
        }
        ENV.UI.Form _myFormOfhostConntrollerForFramesOnlyRRRRR;
        public override void SetForm(Firefly.Box.UI.Form form)
        {
            _myFormOfhostConntrollerForFramesOnlyRRRRR = form as ENV.UI.Form;
            if (_myFormOfhostConntrollerForFramesOnlyRRRRR != null)
            {
                _myFormOfhostConntrollerForFramesOnlyRRRRR.YouAreInAFrame();
                if (_myFormOfhostConntrollerForFramesOnlyRRRRR.UserStateIdentifier != null)
                    _myFormOfhostConntrollerForFramesOnlyRRRRR.UserStateIdentifier += ".FrameForm.";
            }
            base.SetForm(form);
        }
        internal void SetController(ENV.AbstractUIController controller, System.Action<Firefly.Box.UI.SubFormActivationTrigger> runCommand)
        {
            base.SetController(controller._uiController, trigger =>
                                            {
                                                controller.YouAreSetToRunWithinSubForm();
                                                if (trigger == SubFormActivationTrigger.Shown)
                                                {
                                                    foreach (var c in controller.Columns)
                                                    {
                                                        var ec = c as ENV.Utilities.IENVColumn;
                                                        if (ec != null && ec.IsParameter)
                                                        {
                                                            ForceReloadDataOnRefreshSubForm = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                                ENV.Advanced.LevelProvider.StartSubformRunContext(
                                                    () =>
                                                    {
                                                        switch (trigger)
                                                        {
                                                            case Firefly.Box.UI.SubFormActivationTrigger.Recompute:
                                                                controller._subformExecMode = 2;
                                                                break;
                                                            case Firefly.Box.UI.SubFormActivationTrigger.Shown:
                                                                controller._subformExecMode = 1;
                                                                break;
                                                            case Firefly.Box.UI.SubFormActivationTrigger.UserActivation:
                                                                controller._subformExecMode = 0;
                                                                break;
                                                        }
                                                        _controllerRunning = true;
                                                        try
                                                        {
                                                            runCommand(trigger);
                                                        }
                                                        finally
                                                        {
                                                            _controllerRunning = false;
                                                        }
                                                    }, trigger);
                                            }, () =>
                                                   {
                                                       try
                                                       {
                                                           controller._inSubformReload = true;

                                                           runCommand(
                                                               Firefly.Box.UI.SubFormActivationTrigger.Recompute);

                                                       }
                                                       finally
                                                       {
                                                           controller._inSubformReload = false;
                                                       }
                                                   });
        }

        public void SetController(object controller, System.Action runCommand)
        {
            var uic = controller as AbstractUIController;
            if (uic != null)
                this.SetController(uic, (Firefly.Box.UI.SubFormActivationTrigger trigger) => runCommand());
            else
            {
                Form.SetSubFormToAttachLoadedFormTo(this);
                try
                {
                    runCommand();
                }
                finally
                {
                    Form.SetSubFormToAttachLoadedFormTo(null);
                }
            }
        }
        public void SetController(object controller, Action<Runnable> run)
        {
            SetController(controller, () => run(new Runnable(controller)));
        }
        bool _autoRefreshSuspended = false;
        bool _lastSetValueOfAutoRefresh = true;
        internal bool SuspendAutoRefresh
        {
            set
            {

                if (value)
                {
                    if (!_autoRefreshSuspended)
                    {
                        var x = AutoRefresh;
                        AutoRefresh = false;
                        _lastSetValueOfAutoRefresh = x;
                    }
                }
                else
                {
                    if (_lastSetValueOfAutoRefresh != base.AutoRefresh)
                        base.AutoRefresh = _lastSetValueOfAutoRefresh;
                }
                _autoRefreshSuspended = value;

            }
            get
            {
                return _autoRefreshSuspended;
            }
        }
        public override bool AutoRefresh
        {
            get
            {

                return base.AutoRefresh;
            }

            set
            {
                _lastSetValueOfAutoRefresh = value;
                if (!_autoRefreshSuspended)
                    base.AutoRefresh = value;
            }
        }
        public override event BindingEventHandler<BooleanBindingEventArgs> BindAutoRefresh
        {
            add
            {
                base.BindAutoRefresh += (s, e) =>
                {
                    value(s, e);
                    if (_autoRefreshSuspended)
                    {
                        _lastSetValueOfAutoRefresh = e.Value;
                        e.Value = false;
                    }
                };
            }
            remove
            {
            }

        }


        public class Runnable
        {
            object _o;

            public Runnable(object o)
            {
                _o = o;
            }

            public void Run(params object[] args)
            {
                ProgramCollection.RunTask(_o, args, null, _o.GetType());
            }
        }

        bool _controllerRunning;
        internal bool IsControllerRunning()
        {
            return _controllerRunning;
        }

        internal void DoControllerLoadForFrame(bool isComponent, ColumnCollection columns)
        {
            if (_myFormOfhostConntrollerForFramesOnlyRRRRR != null)
                _myFormOfhostConntrollerForFramesOnlyRRRRR.DoControllerLoad(isComponent, columns);
        }
    }

}
