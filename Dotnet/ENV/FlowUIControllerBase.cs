using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Flow;
using Firefly.Box.UI;
using Firefly.Box.UI.Advanced;

namespace ENV
{

    public class FlowUIControllerBase : AbstractUIController
    {
        public FlowUIControllerBase()
        {
            SwitchToInsertWhenNoRows = true;
        }



        internal override void _doOnAdvancedSettings(UIController.AdvancedSettings settings)
        {
            _userFlow = new UserFlow(settings.UseUserFlow(), this);
        }

        internal override sealed void _provideColumnsForFilter(FilterForm.AddColumnToFilterForm addColumnFilter)
        {
            var formColumns = new Dictionary<ColumnBase, InputControlBase>();
            base._provideColumnsForFilter(
                (column, icb) =>
                {
                    if (!formColumns.ContainsKey(column))
                        formColumns.Add(column, icb);
                });
            foreach (var column in Columns)
            {
                InputControlBase c;
                formColumns.TryGetValue(column, out c);
                addColumnFilter(column, c);
            }

        }

        public class UserFlow
        {
            Firefly.Box.Flow.UserFlow _flow;
            FlowUIControllerBase _parent;
            CachedControllerManager _cachedControllerManager;
            internal UserFlow(Firefly.Box.Flow.UserFlow flow, FlowUIControllerBase parent)
            {
                _parent = parent;
                _flow = flow;
                _cachedControllerManager = parent._cachedControllerManager;
            }
            public void Add(Action action, Func<bool> condition, Direction direction)
            {
                _flow.Add(action, condition, direction);
            }
            public void Add(Action action, Direction direction)
            {
                _flow.Add(action, direction);
            }
            public void Add(Action action, FlowMode flowMode, Direction direction)
            {
                _flow.Add(action, flowMode, direction);
            }
            public void Add<taskType>(CachedTask<taskType> cachedTask, Action runTask) where taskType : class
            {
                _flow.Add(cachedTask, runTask);
            }
            public void Add(Action action, Func<bool> condition, FlowMode flowMode)
            {
                _flow.Add(action, condition, flowMode);
            }
            public void Add(Action action, Func<bool> condition)
            {
                _flow.Add(action, condition);
            }
            public void Add(Action action, Func<bool> condition, FlowMode flowMode, Direction direction)
            {
                _flow.Add(action, condition, flowMode, direction);
            }
            public void AddCallByIndex(Func<Number> programIndex, Action<ProgramCollection.Runnable> action, Func<bool> condition, FlowMode flowMode, Direction direction)
            {
                var c = _parent._application.AllPrograms.GetCachedByIndex(programIndex, null);
                _flow.Add(c, () => action(c), condition, flowMode, direction);
            }
            public void AddCallByIndex(Func<Number> programIndex, Action<ProgramCollection.Runnable> action, Func<bool> condition, FlowMode flowMode)
            {
                var c = _parent._application.AllPrograms.GetCachedByIndex(programIndex, null);
                _flow.Add(c, () => action(c), condition, flowMode);
            }
            public void AddCallByIndex(Func<Number> programIndex, Action<ProgramCollection.Runnable> action, Func<bool> condition)
            {
                var c = _parent._application.AllPrograms.GetCachedByIndex(programIndex, null);
                _flow.Add(c, () => action(c), condition);
            }
            public void AddCallByIndex(Func<Number> programIndex, Action<ProgramCollection.Runnable> action)
            {
                var c = _parent._application.AllPrograms.GetCachedByIndex(programIndex, null);
                _flow.Add(c, () => action(c));
            }
            public void AddCallByIndex(Func<Number> programIndex, Action<ProgramCollection.Runnable> action, Func<bool> condition, Direction direction)
            {
                var c = _parent._application.AllPrograms.GetCachedByIndex(programIndex, null);
                _flow.Add(c, () => action(c), condition, direction);
            }
            public void AddCallByIndex(Func<Number> programIndex, Action<ProgramCollection.Runnable> action, Direction direction)
            {
                var c = _parent._application.AllPrograms.GetCachedByIndex(programIndex, null);
                _flow.Add(c, () => action(c), direction);
            }
            public void AddCallByIndex(Func<Number> programIndex, Action<ProgramCollection.Runnable> action, FlowMode flowMode, Direction direction)
            {
                var c = _parent._application.AllPrograms.GetCachedByIndex(programIndex, null);
                _flow.Add(c, () => action(c), flowMode, direction);
            }
            public void AddCallByIndex(Func<Number> programIndex, Action<ProgramCollection.Runnable> action, FlowMode flowMode)
            {
                var c = _parent._application.AllPrograms.GetCachedByIndex(programIndex, null);
                _flow.Add(c, () => action(c), flowMode);
            }





            public void AddCallByPublicName(Func<Text> programPublicName, Action<ProgramCollection.Runnable> action, Func<bool> condition, FlowMode flowMode, Direction direction)
            {
                var c = _parent._application.AllPrograms.GetCachedByPublicName(programPublicName, null);
                _flow.Add(c, () => action(c), condition, flowMode, direction);
            }
            public void AddCallControllerFromAnUnreferencedApplication(Func<Text> applicaitonKey, Func<Text> programPublicName, Action<ProgramCollection.Runnable> action, FlowMode flowMode)
            {
                ProgramCollection.MyCachedController c = GetCachedControllerForUnreferencedApplicatio(applicaitonKey, programPublicName);
                _flow.Add(c, () => action(c), flowMode);
            }
            public void AddCallControllerFromAnUnreferencedApplication(Func<Text> applicaitonKey, Func<Text> programPublicName, Action<ProgramCollection.Runnable> action, Func<bool> condition, FlowMode flowMode, Direction direction)
            {
                ProgramCollection.MyCachedController c = GetCachedControllerForUnreferencedApplicatio(applicaitonKey, programPublicName);
                _flow.Add(c, () => action(c), condition, flowMode, direction);
            }
            public void AddCallControllerFromAnUnreferencedApplication(Func<Text> applicaitonKey, Func<Text> programPublicName, Action<ProgramCollection.Runnable> action, FlowMode flowMode, Direction direction)
            {
                ProgramCollection.MyCachedController c = GetCachedControllerForUnreferencedApplicatio(applicaitonKey, programPublicName);
                _flow.Add(c, () => action(c),  flowMode, direction);
            }
            public void AddCallControllerFromAnUnreferencedApplication(Func<Text> applicaitonKey, Func<Text> programPublicName, Action<ProgramCollection.Runnable> action, Func<bool> condition, FlowMode flowMode)
            {
                ProgramCollection.MyCachedController c = GetCachedControllerForUnreferencedApplicatio(applicaitonKey, programPublicName);
                _flow.Add(c, () => action(c), condition, flowMode);
            }
            public void AddCallControllerFromAnUnreferencedApplication(Func<Text> applicaitonKey, Func<Text> programPublicName, Action<ProgramCollection.Runnable> action, Func<bool> condition)
            {
                ProgramCollection.MyCachedController c = GetCachedControllerForUnreferencedApplicatio(applicaitonKey, programPublicName);
                _flow.Add(c, () => action(c), condition);
            }
            public void AddCallControllerFromAnUnreferencedApplication(Func<Text> applicaitonKey, Func<Text> programPublicName, Action<ProgramCollection.Runnable> action)
            {
                ProgramCollection.MyCachedController c = GetCachedControllerForUnreferencedApplicatio(applicaitonKey, programPublicName);
                _flow.Add(c, () => action(c));
            }
            public void AddCallControllerFromAnUnreferencedApplication(Func<Text> applicaitonKey, Func<Text> programPublicName, Action<ProgramCollection.Runnable> action, Func<bool> condition, Direction direction)
            {
                ProgramCollection.MyCachedController c = GetCachedControllerForUnreferencedApplicatio(applicaitonKey, programPublicName);
                _flow.Add(c, () => action(c), condition, direction);
            }
            public void AddCallControllerFromAnUnreferencedApplication(Func<Text> applicaitonKey, Func<Text> programPublicName, Action<ProgramCollection.Runnable> action, Direction direction)
            {
                ProgramCollection.MyCachedController c = GetCachedControllerForUnreferencedApplicatio(applicaitonKey, programPublicName);
                _flow.Add(c, () => action(c), direction);
            }
            
 


            private ProgramCollection.MyCachedController GetCachedControllerForUnreferencedApplicatio(Func<Text> applicaitonKey, Func<Text> programPublicName)
            {
                ProgramCollection.MyCachedController oneToUse = null;
                var c = new ProgramCollection.MyCachedController(() =>
                {
                    if (oneToUse == null)
                    {
                        ApplicationControllerBase.InternalRunProgramFromAnUnreferencedApplication(applicaitonKey(), programPublicName(), app => oneToUse = app.AllPrograms.GetCachedByPublicName(programPublicName, null), x => x(_parent._application));
                    }
                    return oneToUse.GetTheType();
                }, () =>
                {
                    return oneToUse.Instance;
                }, null);
                return c;
            }

            public void AddCallByPublicName(Func<Text> programPublicName, Action<ProgramCollection.Runnable> action, Func<bool> condition, FlowMode flowMode)
            {
                var c = _parent._application.AllPrograms.GetCachedByPublicName(programPublicName, null);
                _flow.Add(c, () => action(c), condition, flowMode);
            }
            public void AddCallByPublicName(Func<Text> programPublicName, Action<ProgramCollection.Runnable> action, Func<bool> condition)
            {
                var c = _parent._application.AllPrograms.GetCachedByPublicName(programPublicName, null);
                _flow.Add(c, () => action(c), condition);
            }
            public void AddCallByPublicName(Func<Text> programPublicName, Action<ProgramCollection.Runnable> action)
            {
                var c = _parent._application.AllPrograms.GetCachedByPublicName(programPublicName, null);
                _flow.Add(c, () => action(c));
            }
            public void AddCallByPublicName(Func<Text> programPublicName, Action<ProgramCollection.Runnable> action, Func<bool> condition, Direction direction)
            {
                var c = _parent._application.AllPrograms.GetCachedByPublicName(programPublicName, null);
                _flow.Add(c, () => action(c), condition, direction);
            }
            public void AddCallByPublicName(Func<Text> programPublicName, Action<ProgramCollection.Runnable> action, Direction direction)
            {
                var c = _parent._application.AllPrograms.GetCachedByPublicName(programPublicName, null);
                _flow.Add(c, () => action(c), direction);
            }
            public void AddCallByPublicName(Func<Text> programPublicName, Action<ProgramCollection.Runnable> action, FlowMode flowMode, Direction direction)
            {
                var c = _parent._application.AllPrograms.GetCachedByPublicName(programPublicName, null);
                _flow.Add(c, () => action(c), flowMode, direction);
            }
            public void AddCallByPublicName(Func<Text> programPublicName, Action<ProgramCollection.Runnable> action, FlowMode flowMode)
            {
                var c = _parent._application.AllPrograms.GetCachedByPublicName(programPublicName, null);
                _flow.Add(c, () => action(c), flowMode);
            }


            public void Add(Action action)
            {
                _flow.Add(action);
            }
            public void Add(Action action, FlowMode flowMode)
            {
                _flow.Add(action, flowMode);
            }

            public void Add<controller>(CachedTask<controller> cachedTask, Action runTask, Func<bool> condition, FlowMode flowMode, Direction direction) where controller : class
            {
                _flow.Add(cachedTask, runTask, condition, flowMode, direction);
            }
            public void Add<controller>(CachedTask<controller> cachedTask, Action runTask, Func<bool> condition, Direction direction) where controller : class
            {
                _flow.Add(cachedTask, runTask, condition, direction);
            }
            public void Add<controller>(CachedTask<controller> cachedTask, Action runTask, Func<bool> condition) where controller : class
            {
                _flow.Add(cachedTask, runTask, condition);
            }
            public void Add<controller>(CachedTask<controller> cachedTask, Action runTask, FlowMode flowMode, Direction direction) where controller : class
            {
                _flow.Add(cachedTask, runTask, flowMode, direction);
            }
            public void Add<controller>(CachedTask<controller> cachedTask, Action runTask, Direction direction) where controller : class
            {
                _flow.Add(cachedTask, runTask, direction);
            }
            public void Add<controller>(CachedTask<controller> cachedTask, Action runTask, FlowMode flowMode) where controller : class
            {
                _flow.Add(cachedTask, runTask, flowMode);
            }


            public void Add<controller>(Action<controller> runTask, Func<bool> condition, FlowMode flowMode, Direction direction) where controller : class
            {
                var cachedTask = (CachedController<controller>)_cachedControllerManager.GetTheCachedController<controller>();
                _flow.Add(cachedTask, () => runTask(cachedTask.Instance), condition, flowMode, direction);
            }
            public void Add<controller>(Action<controller> runTask, Func<bool> condition, FlowMode flowModel) where controller : class
            {
                var cachedTask = (CachedController<controller>)_cachedControllerManager.GetTheCachedController<controller>();
                _flow.Add(cachedTask, () => runTask(cachedTask.Instance), condition, flowModel);
            }
            public void Add<controller>(Action<controller> runTask) where controller : class
            {
                var cachedTask = (CachedController<controller>)_cachedControllerManager.GetTheCachedController<controller>();
                _flow.Add(cachedTask, () => runTask(cachedTask.Instance));
            }
            public void Add<controller>(Action<controller> runTask, Func<bool> condition, Direction direction) where controller : class
            {
                var cachedTask = (CachedController<controller>)_cachedControllerManager.GetTheCachedController<controller>();
                _flow.Add(cachedTask, () => runTask(cachedTask.Instance), condition, direction);
            }

            public void Add<controller>(Action<controller> runTask, Func<bool> condition) where controller : class
            {
                var cachedTask = (CachedController<controller>)_cachedControllerManager.GetTheCachedController<controller>();
                _flow.Add(cachedTask, () => runTask(cachedTask.Instance), condition);
            }

            public void Add<controller>(Action<controller> runTask, FlowMode flowMode, Direction direction) where controller : class
            {
                var cachedTask = (CachedController<controller>)_cachedControllerManager.GetTheCachedController<controller>();
                _flow.Add(cachedTask, () => runTask(cachedTask.Instance), flowMode, direction);
            }

            public void Add<controller>(Action<controller> runTask, Direction direction) where controller : class
            {
                var cachedTask = (CachedController<controller>)_cachedControllerManager.GetTheCachedController<controller>();
                _flow.Add(cachedTask, () => runTask(cachedTask.Instance), direction);
            }

            public void Add<controller>(Action<controller> runTask, FlowMode flowMode) where controller : class
            {
                var cachedTask = (CachedController<controller>)_cachedControllerManager.GetTheCachedController<controller>();
                _flow.Add(cachedTask, () => runTask(cachedTask.Instance), flowMode);
            }

            public void Add(Control control)
            {
                _flow.Add(control);
            }

            public void Add(SubForm subForm)
            {
                _flow.Add(subForm);
            }

            public void Add<taskType>(CachedTask<taskType> cachedTask, Action runTask, Func<bool> condition, FlowMode flowMode) where taskType : class
            {
                _flow.Add(cachedTask, runTask, condition, flowMode);
            }

            public void EndBlock()
            {
                _flow.EndBlock();
            }

            public void RunExpandEventOfParkedColumnEvenIfRaisedByOtherControls(Action runExpandEvent)
            {
                _flow.RunExpandEventOfParkedColumnEvenIfRaisedByOtherControls(runExpandEvent);
            }

            public void StartBlock()
            {
                _flow.StartBlock();
            }

            public void StartBlock(FlowMode flowMode)
            {
                _flow.StartBlock(flowMode);
            }

            public void StartBlock(Direction direction)
            {
                _flow.StartBlock(direction);
            }

            public void StartBlock(FlowMode flowMode, Direction direction)
            {
                _flow.StartBlock(flowMode, direction);
            }

            public void StartBlock(Func<bool> condition)
            {
                _flow.StartBlock(condition);
            }

            public void StartBlock(Func<bool> condition, Direction direction)
            {
                _flow.StartBlock(condition, direction);
            }

            public void StartBlock(Func<bool> condition, FlowMode flowMode)
            {
                _flow.StartBlock(condition, flowMode);
            }

            public void StartBlock(Func<bool> condition, FlowMode flowMode, Direction direction)
            {
                _flow.StartBlock(condition, flowMode, direction);
            }

            public void StartBlockElse(Func<bool> condition)
            {
                _flow.StartBlockElse(condition);
            }

            public void StartBlockElse()
            {
                _flow.StartBlockElse();
            }
        }
        UserFlow _userFlow;
        protected internal UserFlow Flow
        {
            get { return _userFlow; }
        }
    }
}
