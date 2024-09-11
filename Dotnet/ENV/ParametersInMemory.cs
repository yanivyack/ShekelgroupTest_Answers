using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using ENV.BackwardCompatible;
using Firefly.Box;
using ENV.Data.DataProvider;
using ENV.Data;
using ENV.Remoting;

namespace ENV
{
    public class ParametersInMemory
    {
        public static ParametersInMemory Instance
        {
            get
            {
                ParametersInMemory result = Context.Current[typeof(ParametersInMemory)] as ParametersInMemory;
                if (result == null)
                {
                    result = new ParametersInMemory();
                    Context.Current[typeof(ParametersInMemory)] = result;
                }
                return result;
            }
        }
        static readonly ParametersInMemory _sharedInstance = new ParametersInMemory();
        public static ParametersInMemory SharedInstance
        {
            get
            {
                if (!SharedInstanceShouldBeSeperatedByThreadIdAndAppName)
                    return _sharedInstance;
                Dictionary<int, ParametersInMemory> innerDict;
                var appName = (UserMethods.Instance.GetTextParam("APPNAME") ?? Text.Empty).ToString();
                var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                ParametersInMemory result;
                if (!_sharedInstancePerThreadAndAppName.TryGetValue(appName, out innerDict))
                {
                    lock (_sharedInstancePerThreadAndAppName)
                    {
                        if (!_sharedInstancePerThreadAndAppName.TryGetValue(appName, out innerDict))
                        {
                            _sharedInstancePerThreadAndAppName.Add(appName,
                                                                   innerDict = new Dictionary<int, ParametersInMemory>());
                            innerDict.Add(threadId, result = new ParametersInMemory());
                            return result;
                        }
                    }
                }
                if (!innerDict.TryGetValue(threadId, out result))
                {
                    lock (innerDict)
                    {
                        if (!innerDict.TryGetValue(threadId, out result))
                        {
                            innerDict.Add(threadId, result = new ParametersInMemory());
                        }
                    }
                }
                return result;
            }
        }
        static Dictionary<string, Dictionary<int, ParametersInMemory>> _sharedInstancePerThreadAndAppName =
            new Dictionary<string, Dictionary<int, ParametersInMemory>>();
        public static bool SharedInstanceShouldBeSeperatedByThreadIdAndAppName { get; set; }
        public object Get(Text paramName)
        {
            if (paramName == null)
                return null;
            object result;

            paramName = paramName.TrimEnd().ToUpper(CultureInfo.InvariantCulture);
            if (_memoryParams.ContainsKey(paramName))
                result = _memoryParams[paramName];
            else
                result = _outerValueProvide(paramName);
            return result;


        }



        internal static bool GetFromWebForTesting = false;

        internal Func<string, string> _outerValueProvide =
           paramName =>
           {
               if (System.Web.HttpContext.Current != null || GetFromWebForTesting)
               {
                   if (paramName == "MG_POST_BODY")
                       return System.Web.HttpContext.Current.Request.Form.ToString();
                   try
                   {
                       var parValue = IO.WebWriter.GetRequestValues(paramName);
                       if (parValue != null && parValue.Length > 0)
                           return parValue[parValue.Length - 1] == "" ? null : parValue[parValue.Length - 1];
                   }
                   catch { }
               }
               return null;
           };


        Dictionary<string, object> _memoryParams = new Dictionary<string, object>();

        public bool Set(Text paramName, object value)
        {
            Text t;
            paramName = paramName.TrimEnd().ToUpper(CultureInfo.InvariantCulture);
            if (Text.TryCast(value, out t))
            {
                if (t != null && t.Length == 0)
                {
                    if (!UserSettings.Version8Compatible)
                        value = null;
                    else
                    {
                        object previousValue;
                        if (_memoryParams.TryGetValue(paramName, out previousValue))
                        {
                            if (previousValue == null)
                                value = null;
                        }
                        else value = null;

                    }
                }
            }



            if (!_memoryParams.ContainsKey(paramName))
            {
                lock (_memoryParams)
                {
                    if (!_memoryParams.ContainsKey(paramName))
                    {
                        _memoryParams.Add(paramName, value);
                        return true;
                    }
                }
            }
            _memoryParams[paramName] = value;
            return true;

        }

        internal void ClearParametersThatExistInOuterValueProvider()
        {

            foreach (var key in new List<string>(_memoryParams.Keys).ToArray())
            {
                var result = _outerValueProvide(key);
                if (result != null)
                    _memoryParams.Remove(key);
            }
        }

        public void DuplicateTo(ParametersInMemory to)
        {
            foreach (var memoryParam in _memoryParams)
            {
                to._memoryParams.Add(memoryParam.Key, memoryParam.Value);
            }
        }

        public void Clear()
        {
            _memoryParams.Clear();
        }

        internal Copy CreateCopy()
        {
            return new Copy(_memoryParams);
        }
        internal class Copy
        {
            Dictionary<string, object> _memoryParams;
            public Copy(Dictionary<string, object> p)
            {
                _memoryParams = new Dictionary<string, object>(p);
                if (HttpContext.Current != null)
                {
                    var c = HttpContext.Current.Request;
                    if (c != null)
                    {
                        foreach (var item in c.Form.AllKeys)
                        {
                            if (!_memoryParams.ContainsKey(item.ToUpper(CultureInfo.InvariantCulture)))
                                _memoryParams.Add(item.ToUpper(CultureInfo.InvariantCulture),
                                    ParametersInMemory.Instance._outerValueProvide(item));
                        }
                        foreach (var item in c.Params.AllKeys)
                        {
                            if (!_memoryParams.ContainsKey(item.ToUpper(CultureInfo.InvariantCulture)))
                                _memoryParams.Add(item.ToUpper(CultureInfo.InvariantCulture),
                                    ParametersInMemory.Instance._outerValueProvide(item));
                        }
                        foreach (var item in c.Cookies.AllKeys)
                        {
                            if (!_memoryParams.ContainsKey(item.ToUpper(CultureInfo.InvariantCulture)))
                                _memoryParams.Add(item.ToUpper(CultureInfo.InvariantCulture),
                                    ParametersInMemory.Instance._outerValueProvide(item));
                        }

                    }
                }
            }


            internal void ApplyTo(ParametersInMemory parametersInMemory)
            {
                foreach (var item in _memoryParams)
                {
                    parametersInMemory._memoryParams.Add(item.Key, item.Value);
                }
            }
        }
        public void Display()
        {
            var e = new ENV.Data.Entity("ParametersInMemory", new MemoryDatabase());
            TextColumn name, value;
            e.Columns.Add(name = new TextColumn("Name", "255"), value = new TextColumn("Value", "1000"));
            e.SetPrimaryKey(new[] { name });
            foreach (var item in _memoryParams)
            {
                e.Insert(() =>
                {
                    name.Value = item.Key;
                    if (item.Value == null)
                        value.Value = "null";
                    else
                        value.Value = item.Value.ToString();
                });
            }
            new ENV.Utilities.EntityBrowser(e, false).Run();
        }

        internal byte[] Serialize()
        {
            var ser = new ClientParameterManager();
            var x = new Dictionary<string, object>();
            foreach (var item in _memoryParams)
            {
                x.Add(item.Key, ser.Pack(item.Value));
            }
            return ENV.UserMethods.Instance.Serialize(x);
        }

        internal bool Deserialize(byte[] bytes)
        {
            var x = ENV.UserMethods.Instance.Deserialize<Dictionary<string, object>>(bytes);
            if (x != null)
            {
                var ser = new ServerParameterManager();
                foreach (var item in x)
                {
                    this.Set(item.Key, ser.UnPack(item.Value));
                }
                return true;
            }
            return false;
        }
    }
}