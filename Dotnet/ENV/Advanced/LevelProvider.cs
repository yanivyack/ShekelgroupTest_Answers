using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ENV.UI;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;
using Firefly.Box.UI;
using Firefly.Box.UI.Advanced;

namespace ENV.Advanced
{
    class LevelProvider
    {
        string _level, _mainLevel;
        bool _isModuleController;
        ITask _task;
        object _controller;

        public LevelProvider(bool isModuleController, ITask task,object controller)
        {
            _controller = controller;
            _isModuleController = isModuleController;
            _task = task;
            _level = _isModuleController ? "MP" : "TP";
            _mainLevel = "TP";
        }
        internal string GetMainLevel() {
            return _mainLevel;
        }

        public IDisposable StartContext(string level, string mainLevel,string name)
        {
            var x = new myDisposable(this,ENV.Utilities.Profiler.Level(_controller,name));
            _level = level;
            _mainLevel = mainLevel;
            return new TaskInCallStack(_task, x,this);

        }
        internal static bool IsCurrentHandlerIn(ITask task)
        {
            if (TopOfStack.Value == null)
                return false;
            if (TopOfStack.Value._task == task && TopOfStack.Value._level.StartsWith("H"))
                return true;
            return false;
        }
        static ContextStatic<LevelProvider> TopOfStack = new ContextStatic<LevelProvider>(()=>null);
        class TaskInCallStack : IDisposable
        {
            ITask _task;
            IDisposable _context;
            LevelProvider _previous;
            public TaskInCallStack(ITask task, IDisposable context,LevelProvider current)
            {
                _task = task;
                _context = context;
                _previous = TopOfStack.Value;
                TopOfStack.Value = current;
            }

            public void Dispose()
            {
                TopOfStack.Value = _previous;
                _context.Dispose();
            }
        }

        public IDisposable StartContext(string level,string mainLevel =null)
        {
            return StartContext(level,mainLevel?? _mainLevel,level);
        }


        class myDisposable : IDisposable
        {
            LevelProvider _parent;
            string _originalLevel;
            string _mainLevel;
            IDisposable _trace;
            IHaveUserMethods _controller;
            bool _shouldCloseBlockLoop = false;
            public myDisposable(LevelProvider parent,IDisposable trace)
            {
                _trace = trace;
                _parent = parent;
                _originalLevel = _parent._level;
                _mainLevel = _parent._mainLevel;
                _controller = _parent._controller as IHaveUserMethods;
                if (_controller != null)
                {
                    _shouldCloseBlockLoop =  _controller.GetUserMethods().StartBlockLoopIfHasActiveLoop();
                }
                
            }
            public void Dispose()
            {
                _parent._level = _originalLevel;
                _parent._mainLevel = _mainLevel;
                _trace.Dispose();
                if (_shouldCloseBlockLoop)
                    _controller.GetUserMethods().EndBlockLoop();
            }
        }

        public IDisposable EnterRow()
        {
            return StartContext("RP", "RP","OnEnterRow");
        }

        public IDisposable LeaveRow()
        {
            return StartContext("RS", "RS","OnLeaveRow");
        }
        public IDisposable SavingRow()
        {
            return StartContext("RS", "RS", "OnSavingRow");
        }

        public void AfterSavingRow()
        {
            StartContext("RM", "RS", null);
        }

        public IDisposable Start()
        {
            return StartContext(( "T") + "P", "TP","OnStart");
        }
        public void BeforeStart()
        {
             StartContext((_isModuleController ? "M" : "T") + "P", "TP", null);
        }

        public IDisposable End()
        {
            return StartContext(("T") + "S", "TS","OnEnd");
        }

        public static void StartEnterControlContext(Action action, System.Windows.Forms.Control control)
        {
            StartEnterControlContext(action, control, "CP");
        }
        public static void StartInputValidationControlContext(Action action, System.Windows.Forms.Control control)
        {
            StartEnterControlContext(action, control, "CV");
        }
        public static void StartLeaveControlContext(Action action, System.Windows.Forms.Control control)
        {
            StartEnterControlContext(action, control, "CS");
        }
        public static void StartChangeControlContext(Action action, System.Windows.Forms.Control control)
        {
            StartEnterControlContext(action, control, "CC");
        }
        public static void StartSubformRunContext(Action action, SubFormActivationTrigger trigger)
        {
            if (trigger==SubFormActivationTrigger.Shown)
                StartEnterControlContext(action, "SUBFORM_Shown" ,"RP");
            else
                StartEnterControlContext(action, "SUBFORM");
        }
        static void StartEnterControlContext(Action action, System.Windows.Forms.Control control, string s)
        {
            StartEnterControlContext(action, s + "_" + control.Tag);
        }

        static void StartEnterControlContext(Action action, string s,string mainLevel=null)
        {
            var lp =
                GetLevelProvider(
                    Firefly.Box.Context.Current.ActiveTasks[Firefly.Box.Context.Current.ActiveTasks.Count - 1]);
            if (action != null)
                if (lp != null)
                    using (lp.StartContext(s, mainLevel))
                    {
                        action();
                    }
                else
                    action();

        }
        static LevelProvider GetLevelProvider(ITask task)
        {
            LevelProvider result = null;
            ControllerBase.SendInstanceBasedOnTaskAndCallStack(task,y => result = y._levelProvider,app=>result = app._levelProvider);
            return result;
        }

        public IDisposable ColumnChange(ColumnBase column)
        {
            return StartContext("VC_" + column.Caption);
        }

        public static Text GetLevelOf(ITask task)
        {
            var x = GetLevelProvider(task);
            if (x != null)
            {
                if (x._level == "SUBFORM_Shown")
                    return x._controller.GetType().GetMethod("OnEnterRow", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) != null ? 
                        "RP" : "SUBFORM";
                return x._level;
            }
            if (task is ModuleController)
                return "MP";
            return "TP";
        }
        public static Text GetMainLevelOf(ITask task)
        {
            var x = GetLevelProvider(task);
            if (x != null)
                return x._mainLevel;
            if (task is ModuleController)
                return "TP";
            return "TP";
        }

        internal bool IsTheTopGenerationController()
        {
            var ac = Firefly.Box.Context.Current.ActiveTasks;
            return ac[ac.Count-1] == _task;

        }

        
    }
}
