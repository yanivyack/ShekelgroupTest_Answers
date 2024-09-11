using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ENV;

namespace ENV.Utilities
{
    public class WindowsServiceHelper
    {
        private readonly string _serviceName;
        public WindowsServiceHelper(string serviceName)
        {
            _serviceName = serviceName;
        }

        public  string GetServiceName()
        {
            return _serviceName+" " + _controllerName;
        }

        public  string GetControllerName()
        {
            return _controllerName;
        }


        static string _controllerName = "test";
        public  void SetControllerName(string name)
        {
            if (!string.IsNullOrEmpty(name))
                _controllerName = name;
        }
        private  Dictionary<string, WindowsServiceController> _registeredControllers =
            new Dictionary<string, WindowsServiceController>();
        void InitBasedOnControllersFromAssembly(Assembly assembly)
        {
            if (assembly == null)
                return;


            foreach (var type in assembly.GetTypes())
            {
                if (typeof(WindowsServiceController).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    try
                    {
                        var x = (WindowsServiceController)System.Activator.CreateInstance(type);
                        _registeredControllers.Add(x.GetName(), x);
                    }
                    catch (Exception ex)
                    {
                        ENV.ErrorLog.WriteToLogFile(ex);
                    }
                }
            }
        }

        protected virtual Assembly GetControllersAssembly()
        {
            return null;
        }


        public void OnServiceStart(Action<string> reportError)
        {
            ENV.Common.SuppressDialogs();
            Firefly.Box.Context.Current.SetNonUIThread();

            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            InitBasedOnControllersFromAssembly(GetControllersAssembly());
            WindowsServiceController c = null;
            if (_registeredControllers.TryGetValue(_controllerName, out c))
            {
                ENV.Common.RunOnNewThread(() =>
                {
                    Firefly.Box.Context.Current.SetNonUIThread();
                    while (!_stop)
                    {
                        try
                        {
                            c.Execute();
                            if (!_stop)
                                ENV.UserMethods.Instance.Delay(5);

                        }

                        catch (Exception e)
                        {
                            ENV.ErrorLog.WriteToLogFile(e);
                        }
                    }
                    

                });
            }
            else
                throw new Exception("Couldn't find controller named " + _controllerName);

        }

        private static bool _stop;
        public  void OnServiceStop()
        {
            _stop = true;
        }
    }
    public abstract class WindowsServiceController
    {
        public abstract string GetName();
        public abstract void Execute();

    }
}
