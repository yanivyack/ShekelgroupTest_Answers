using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ENV
{
    public class AbstractFactory
    {
        static Dictionary<string, Dictionary<string, Type>> _cache = new Dictionary<string, Dictionary<string, Type>>();

        public static object CreateInstance(Type t)
        {
            if (!t.IsAbstract && !t.IsInterface)
                return System.Activator.CreateInstance(t);
            return CreateInstance(t, () =>
            {
                var ns = t.Namespace;
                var baseName = t.Assembly.GetName().Name;
                if (baseName.EndsWith("Base"))
                {
                    baseName = baseName.Remove(baseName.Length - 4);
                }
                if (baseName.EndsWith("Interfaces"))
                    baseName = baseName.Remove(baseName.Length - 10);
                if (ns.StartsWith(baseName))
                {
                    if (ns.Length > baseName.Length)
                    {
                        var i = ns.IndexOf('.', baseName.Length + 1);
                        if (i > 0)
                            ns = ns.Remove(i);
                    }

                }

                string fn = GuessTypeName(t);

                return () => CreateInstance(ns, fn);
            });
        }
        public static T CreateInstance<T>()
        {
            return (T)CreateInstance(typeof(T));
        }

        public static void AddFactory(Type requestedType, Type implementationType)
        {
            _factoryPerType.Add(requestedType, () => Activator.CreateInstance(implementationType));
        }
        public static void AddFactory(Type requestedType, Func<object> factory)
        {
            _factoryPerType.Add(requestedType, factory);
        }

        internal static void Clear()
        {
            _cache.Clear();
            _factoryPerType.Clear();

        }

        public static void AddFactory(Type requestedType, string implementationsAssemblyName, string implementationFullTypeName)
        {
            _factoryPerType.Add(requestedType, () => CreateInstance(implementationsAssemblyName, implementationFullTypeName));
        }

        private static string GuessTypeName(Type t)
        {
            var fn = t.FullName + _coreSuffix;
            if (t.IsInterface && t.Name.StartsWith("I"))
            {
                fn = t.FullName;
                fn = fn.Remove(fn.LastIndexOf(".I") + 1, 1);
            }

            return fn;
        }

        public static T Create<T>()
        {
            return CreateInstance<T>();
        }

        public static T CreateInstance<T>(string assemblyName)
        {
            var t = typeof(T);
            return (T)CreateInstance(t, () => () => CreateInstance(assemblyName, GuessTypeName(t)));
        }
        public static T CreateInstance<T>(string assemblyName, string typeFullName)
        {
            var t = typeof(T);
            return (T)CreateInstance(t, () => () => CreateInstance(assemblyName, typeFullName));
        }


        static Dictionary<Type, Func<object>> _factoryPerType = new Dictionary<Type, Func<object>>();
        static string _coreSuffix = "Core";

        public static object CreateInstance(Type t, Func<Func<object>> provideFactory)
        {
            Func<object> result;
            if (!_factoryPerType.TryGetValue(t, out result))
            {
                lock (_factoryPerType)
                {
                    if (!_factoryPerType.TryGetValue(t, out result))
                    {
                        _factoryPerType.Add(t, result = provideFactory());
                    }
                }
            }
            return result();
        }
        static bool _figuredOutEntryPath = false;
        static string _entryPath = null;
        static bool AssemblyExists(string ass)
        {
            if (File.Exists(ass + ".dll") || File.Exists(ass + ".exe"))
                return true;
            if (!_figuredOutEntryPath)
            {
                _figuredOutEntryPath = true;
                var x = System.Reflection.Assembly.GetExecutingAssembly();
                if (x != null)
                {
                    _entryPath = Path.GetDirectoryName(x.CodeBase).Substring(6);
                }
            }
            if (_entryPath == null)
                return false;
            var nameInEntry = Path.Combine(_entryPath, ass);
            return File.Exists(nameInEntry+ ".dll") && !File.Exists(nameInEntry+ ".exe");
        }

        public static object CreateInstance(string assembly, string fullTypeName)
        {
            Dictionary<string, Type> typeCache;
            if (!_cache.TryGetValue(assembly, out typeCache))
            {
                lock (_cache)
                {
                    if (!_cache.TryGetValue(assembly, out typeCache))
                    {
                        typeCache = new Dictionary<string, Type>();
                        _cache.Add(assembly, typeCache);
                    }
                }
            }
            Type t;
            if (typeCache.TryGetValue(fullTypeName, out t))
            {
                return System.Activator.CreateInstance(t);
            }
            lock (_cache)
            {

                if (typeCache.TryGetValue(fullTypeName, out t))
                {
                    return System.Activator.CreateInstance(t);
                }
                try
                {
                    if (string.IsNullOrEmpty(AlternativeDllPath))
                    {
                        var ass = assembly;
                        int loc = -1;
                        while (!AssemblyExists(ass )  && (loc = ass.LastIndexOf(".")) >= 0)
                        {
                            ass = ass.Remove(loc);
                            
                        }
                        if (loc < 0)
                            ass = assembly;


                        var x = System.Activator.CreateInstance(ass, fullTypeName).Unwrap();
                        typeCache.Add(fullTypeName, x.GetType());
                        return x;
                    }
                    else
                    {
                        var key = assembly.ToUpper();
                        Assembly ass;
                        if (!_loadedAssemblies.TryGetValue(key, out ass))
                        {
                            lock (_loadedAssemblies)
                            {
                                if (!_loadedAssemblies.TryGetValue(key, out ass))
                                {
                                    string fileName = null;
                                    fileName = FileExists(assembly);
                                    if (fileName == null && !string.IsNullOrEmpty(AlternativeDllPath))
                                        fileName = FileExists(Path.Combine(AlternativeDllPath, assembly));
                                    if (fileName == null)
                                        throw new FileNotFoundException();
                                    ass = Assembly.LoadFrom(fileName);
                                    _loadedAssemblies.Add(key, ass);
                                }
                            }
                        }
                        var x = ass.CreateInstance(fullTypeName);
                        typeCache.Add(fullTypeName, x.GetType());
                        return x;
                    }
                }
                catch (FileNotFoundException ex)
                {
                    if (WindowsFormsDesignMode)
                        return null;
                    throw new FileNotFoundException(string.Format("Couldn't find assembly \"{0}\"", assembly), ex);
                }
                catch (Exception ex)
                {
                    if (WindowsFormsDesignMode)
                        return null;
                    throw new TypeLoadException("Abstract factory failed to load class '" + fullTypeName + "' in assembly '" + assembly + "'", ex);
                }
            }

        }
        public static string AlternativeDllPath { get; set; }
        internal static bool WindowsFormsDesignMode { get; set; }

        internal static string FileExists(string name)
        {
            foreach (var s in new[] { ".dll", ".exe" })
            {
                var fullName = name + s;
                if (File.Exists(fullName))
                    return fullName;
            }
            return null;
        }
        static Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>();
    }
}
