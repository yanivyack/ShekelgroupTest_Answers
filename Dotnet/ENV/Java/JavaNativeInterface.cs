using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32;

namespace ENV.Java
{
    public class JavaNativeInterface : IDisposable
    {
        private const string JRE_REGISTRY_KEY = @"HKEY_LOCAL_MACHINE\SOFTWARE\JavaSoft\Java Runtime Environment";

        private IntPtr javaClass;
        private IntPtr javaObject;
        private string javaClassName;
        private JavaVM jvm;
        private JavaInvoker env;

        public static string JavaHome { get; set; }
        public static string ClassPath { get; set; }
        public static string JvmOptions { get; set; }

        public struct JNIVersion
        {
            public const int JNI_VERSION_1_2 = 0x00010002;
            public const int JNI_VERSION_1_4 = 0x00010004;
            public const int JNI_VERSION_1_6 = 0x00010006;
        }

        [StructLayout(LayoutKind.Sequential), NativeCppClass]
        public struct JavaVMInitArgs
        {
            public int version;
            public int nOptions;
            public IntPtr options;
            public byte ignoreUnrecognized;
        }


        public JavaNativeInterface()
        {
            var jreVersion = (string)Registry.GetValue(JRE_REGISTRY_KEY, "CurrentVersion", null);

            string jvmPath;

            if (string.IsNullOrEmpty(JavaHome))
            {
                var keyName = Path.Combine(JRE_REGISTRY_KEY, jreVersion);
                jvmPath = Path.GetDirectoryName((string)Registry.GetValue(keyName, "RuntimeLib", null));
            }
            else
                jvmPath = Path.Combine(JavaHome, "bin\\client");

            System.Environment.SetEnvironmentVariable("Path", Environment.GetEnvironmentVariable("Path") + ";" + jvmPath);
            var args = new JavaVMInitArgs();

            switch (Convert.ToInt32((decimal.Parse(jreVersion.Substring(0, 3)) - 1) / 2 * 10))
            {
                case 0:
                    throw new Exception("Unsupported java version.");
                case 1:
                    args.version = JNIVersion.JNI_VERSION_1_2;
                    break;
                case 2:
                    args.version = JNIVersion.JNI_VERSION_1_4;
                    break;
                default:
                    args.version = JNIVersion.JNI_VERSION_1_6;
                    break;
            }

            args.ignoreUnrecognized = 1;

            IntPtr environment;
            IntPtr javaVirtualMachine;
            IntPtr argsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(JavaVMInitArgs)));
            var options = new List<string>();

            try
            {
                int nVMs;
                if (JavaVM.JNI_GetCreatedJavaVMs(out javaVirtualMachine, 1, out nVMs) == 0 && nVMs > 0)
                {
                    lock (_lockCreateJavaVM) { }
                    jvm = new JavaVM(javaVirtualMachine);
                    if (jvm.AttachCurrentThread(out env, argsPtr) != 0)
                        throw new Exception("Failed loading Java Virtual Machine");
                }
                else
                {
                    lock (_lockGetCreatedJavaVM)
                    {
                        if (JavaVM.JNI_GetCreatedJavaVMs(out javaVirtualMachine, 1, out nVMs) == 0 && nVMs > 0)
                        {
                            jvm = new JavaVM(javaVirtualMachine);
                            if (jvm.AttachCurrentThread(out env, argsPtr) != 0)
                                throw new Exception("Failed loading Java Virtual Machine");
                        }
                        else
                        {
                            lock (_lockCreateJavaVM)
                            {
                                var classPathList = new List<string>((!string.IsNullOrEmpty(ClassPath)
                                                ? ClassPath
                                                : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Split(';'));
                                var envClassPath = Environment.GetEnvironmentVariable("CLASSPATH");
                                if (!string.IsNullOrEmpty(envClassPath))
                                    classPathList.AddRange(envClassPath.Split(';'));
                                options.Add("-Djava.class.path=" + string.Join(";", classPathList.ToArray()));
                                if (!string.IsNullOrEmpty(JvmOptions))
                                    options.AddRange(JvmOptions.Split(' '));

                                if (options.Count > 0)
                                {
                                    args.nOptions = options.Count;
                                    args.options = Marshal.AllocHGlobal(IntPtr.Size * 2 * options.Count);
                                    int i = 0;
                                    foreach (var s in options)
                                    {
                                        Marshal.WriteIntPtr(args.options, i * IntPtr.Size, Marshal.StringToHGlobalAnsi(s.Trim()));
                                        i += 2;
                                    }
                                }
                                Marshal.StructureToPtr(args, argsPtr, false);

                                if (JavaVM.JNI_CreateJavaVM(out javaVirtualMachine, out environment, argsPtr) == 0)
                                {
                                    jvm = new JavaVM(javaVirtualMachine);
                                    env = new JavaInvoker(environment);
                                }
                                else
                                    throw new Exception("Failed loading Java Virtual Machine");
                            }
                        }
                    }
                }
            }
            finally
            {
                for (int i = 0; i < options.Count; i += 2)
                    Marshal.FreeHGlobal(Marshal.ReadIntPtr(args.options, i * IntPtr.Size));
                Marshal.FreeHGlobal(args.options);
                Marshal.FreeHGlobal(argsPtr);
            }
        }

        static object _lockGetCreatedJavaVM = new object();
        static object _lockCreateJavaVM = new object();

        public void CreateObject(string className, string contructorSignature, object[] contructorParams)
        {
            javaClassName = className;
            javaClass = env.FindClass(javaClassName);
            if (javaClass == IntPtr.Zero)
                throw new Exception("Java class not found");
            javaObject = _CreateObject(javaClass, contructorSignature, contructorParams);
        }

        IntPtr _CreateObject (IntPtr javaClass, string contructorSignature, object[] contructorParams)
        {
            IntPtr methodId = env.GetMethodId(javaClass, "<init>", contructorSignature);
            return env.NewObject(javaClass, methodId, ParseParameters(contructorSignature, new List<object>(contructorParams)));
        }

        public object CallStaticMethod(string className, string methodName, string methodSignature, List<object> param)
        {
            var c = env.FindClass(className);
            var mi = env.GetStaticMethodID(c, methodName, methodSignature);
            var p = ParseParameters(methodSignature, param);

            var returnType = GetReturnType(methodSignature);
            if (returnType == typeof(int))
                return env.CallStaticIntMethod(c, mi, p);
            if (returnType == typeof(string))
            {
                var jstr = env.CallStaticObjectMethod(c, mi, p);
                var res = env.JStringToString(jstr);
                env.DeleteLocalRef(jstr);
                return res;
            }
            if (returnType == typeof(void))
            {
                env.CallStaticVoidMethod(c, mi, p);
                return null;
            }
            if (returnType == typeof(short))
                return env.CallStaticShortMethod(c, mi, p);
            if (returnType == typeof(double))
                return env.CallStaticDoubleMethod(c, mi, p);
            if (returnType == typeof(float))
                return env.CallStaticFloatMethod(c, mi, p);
            if (returnType == typeof(bool))
                return env.CallStaticBooleanMethod(c, mi, p);

            return env.CallStaticObjectMethod(c, mi, p);
        }

        Type GetReturnType(string sig)
        {
            return GetTypeFromSig(sig.Substring(sig.LastIndexOf(')') + 1));
        }

        Type GetTypeFromSig(string sig)
        {
            if (sig == "I")
                return typeof(int);
            if (sig.StartsWith("Ljava/lang/String"))
                return typeof(string);
            if (sig == "V")
                return typeof(void);
            if (sig == "S")
                return typeof(short);
            if (sig == "D")
                return typeof(double);
            if (sig == "F")
                return typeof(float);
            if (sig == "Z")
                return typeof(bool);
            if (sig == "B")
                return typeof(byte);
            if (sig == "[B")
                return typeof(byte[]);
            if (sig.StartsWith("Ljava/lang/Integer"))
                return typeof(JInteger);
            if (sig.StartsWith("Ljava/lang/Boolean"))
                return typeof(JBoolean);
            return typeof(object);
        }

        public string GetJavaException()
        {
            return env.CatchJavaException();
        }

        JValue[] ParseParameters(string sig, List<object> param)
        {
            var retval = new JValue[param.Count];

            var startIndex = sig.IndexOf('(') + 1;

            for (var i = 0; i < param.Count; i++)
            {
                string paramSig = "";

                if (sig.Substring(startIndex, 1) == "[")
                    paramSig = sig.Substring(startIndex, 2);
                else if (sig.Substring(startIndex, 1) == "L")
                {
                    paramSig = paramSig + sig.Substring(startIndex, sig.IndexOf(';', startIndex) - startIndex);
                    startIndex++;
                }
                else
                    paramSig = paramSig + sig.Substring(startIndex, 1);

                startIndex = startIndex + paramSig.Length;

                var pType = GetTypeFromSig(paramSig);
                if (pType == typeof(string))
                {
                    retval[i] = new JValue() { l = env.NewString(param[i].ToString(), param[i].ToString().Length) };
                }
                else if (param[i] == null)
                {
                    retval[i] = new JValue();
                }
                else if (pType == typeof(int))
                    retval[i] = new JValue() { i = Convert.ToInt32(param[i]) };
                else if (pType == typeof(short))
                    retval[i] = new JValue() { s = Convert.ToInt16(param[i]) };
                else if (pType == typeof(long))
                    retval[i] = new JValue() { j = Convert.ToInt64(param[i]) };
                else if (pType == typeof(bool))
                    retval[i] = new JValue() { z = (((bool)param[i]) ? (byte)1 : (byte)0) };
                else if (pType == typeof(float))
                    retval[i] = new JValue() { f = Convert.ToSingle(param[i]) };
                else if (pType == typeof(double))
                    retval[i] = new JValue() { d = Convert.ToDouble(param[i]) };
                else if (pType == typeof(byte))
                    retval[i] = new JValue() { b = Convert.ToByte(param[i]) };
                else if (pType == typeof(byte[]))
                    retval[i] = new JValue() { l = new JByteArray(env, (byte[])param[i]).Handle };
                else
                {
                    if (paramSig == "Ljava/lang/Integer")
                        retval[i] = new JValue() { l = _CreateObject(env.FindClass("java/lang/Integer"), "(I)V", new object[] { Convert.ToInt32(param[i]) }) };
                    else if (paramSig == "Ljava/lang/Boolean")
                        retval[i] = new JValue() { l = _CreateObject(env.FindClass("java/lang/Boolean"), "(Z)V", new object[] { param[i] }) };
                }
            }
            return retval;
        }

        public object CallMethod(string methodName, string sig, List<object> param)
        {
            IntPtr methodId = env.GetMethodId(javaClass, methodName, sig);
            if (methodId == IntPtr.Zero)
                throw new Exception("Java method does not exist");
            var returnType = GetReturnType(sig);
            if (returnType == typeof(void))
            {
                env.CallVoidMethod(javaObject, methodId, ParseParameters(sig, param));
                return null;
            }
            if (returnType == typeof(byte))
                return env.CallByteMethod(javaObject, methodId, ParseParameters(sig, param));
            if (returnType == typeof(bool))
                return env.CallBooleanMethod(javaObject, methodId, ParseParameters(sig, param));
            if (returnType == typeof(char))
                return env.CallCharMethod(javaObject, methodId, ParseParameters(sig, param));
            if (returnType == typeof(short))
                return env.CallShortMethod(javaObject, methodId, ParseParameters(sig, param));
            if (returnType == typeof(int))
                return env.CallIntMethod(javaObject, methodId, ParseParameters(sig, param));
            if (returnType == typeof(long))
                return env.CallLongMethod(javaObject, methodId, ParseParameters(sig, param));
            if (returnType == typeof(float))
                return env.CallFloatMethod(javaObject, methodId, ParseParameters(sig, param));
            if (returnType == typeof(double))
                return env.CallDoubleMethod(javaObject, methodId, ParseParameters(sig, param));
            if (returnType == typeof(string))
            {
                var jstr = env.CallObjectMethod(javaObject, methodId, ParseParameters(sig, param));
                if (jstr == IntPtr.Zero) return null;
                string res = env.JStringToString(jstr);
                env.DeleteLocalRef(jstr);
                return res;
            }
            if (returnType == typeof(byte[]))
            {
                var x = env.CallObjectMethod(javaObject, methodId, ParseParameters(sig, param));
                if (x == IntPtr.Zero) return null;
                return new JByteArray(env, x).Value;
            }
            if (returnType == typeof(JBoolean))
            {
                var x = env.CallObjectMethod(javaObject, methodId, ParseParameters(sig, param));
                if (x == IntPtr.Zero) return null;
                return env.CallBooleanMethod(x, env.GetMethodId(env.FindClass("java/lang/Boolean"), "booleanValue", "()Z"));
            }
            if (returnType == typeof(JInteger))
            {
                var x = env.CallObjectMethod(javaObject, methodId, ParseParameters(sig, param));
                if (x == IntPtr.Zero) return null;
                return env.CallIntMethod(x, env.GetMethodId(env.FindClass("java/lang/Integer"), "intValue", "()I"));
            }
            return env.CallObjectMethod(javaObject, methodId, ParseParameters(sig, param));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~JavaNativeInterface()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (javaClass != IntPtr.Zero)
            {
                env.DeleteGlobalRef(javaClass);
                javaClass = IntPtr.Zero;
            }

            if (javaObject != IntPtr.Zero)
            {
                env.DeleteLocalRef(javaObject);
                javaObject = IntPtr.Zero;
            }

            if (disposing)
            {
                if (jvm != null)
                {
                    jvm.Dispose();
                    jvm = null;
                }

                if (env != null)
                {
                    env.Dispose();
                    env = null;
                }
            }
        }

        public object GetFieldValue(string fieldName, string signature)
        {
            var fieldId = env.GetFieldID(javaClass, fieldName, signature);
            var returnType = GetReturnType(signature);
            if (returnType == typeof(byte))
                return env.GetByteField(javaObject, fieldId);
            if (returnType == typeof(bool))
                return env.GetBooleanField(javaObject, fieldId);
            if (returnType == typeof(char))
                return env.GetCharField(javaObject, fieldId);
            if (returnType == typeof(short))
                return env.GetShortField(javaObject, fieldId);
            if (returnType == typeof(int))
                return env.GetIntField(javaObject, fieldId);
            if (returnType == typeof(long))
                return env.GetLongField(javaObject, fieldId);
            if (returnType == typeof(float))
                return env.GetFloatField(javaObject, fieldId);
            if (returnType == typeof(double))
                return env.GetDoubleField(javaObject, fieldId);
            if (returnType == typeof(string))
            {
                var jstr = env.GetObjectField(javaObject, fieldId);
                string res = env.JStringToString(jstr);
                env.DeleteLocalRef(jstr);
                return res;
            }
            return env.GetObjectField(javaObject, fieldId);
        }
    }

    public class JByteArray
    {
        HandleRef native;

        JavaInvoker _env;

        public JByteArray(JavaInvoker env, byte[] buf)
        {
            _env = env;
            native = new HandleRef(this, _env.NewByteArray(buf.Length));
            _env.SetByteArrayRegion(native.Handle, 0, buf.Length, buf);
        }

        public JByteArray(JavaInvoker env, IntPtr raw)
        {
            _env = env;
            native = new HandleRef(this, raw);
        }

        public byte[] Value
        {
            get
            {
                byte[] c = new byte[Length];
                _env.GetByteArrayRegion(native.Handle, 0, Length, c);
                return c;
            }
        }

        public IntPtr Handle { get { return native.Handle; } }

        int Length
        {
            get
            {
                return _env.GetArrayLength(native.Handle);
            }
        }

        ~JByteArray()
        {
            _env.DeleteLocalRef(native.Handle);
        }
    }

    public class JBoolean { }
    public class JInteger { }
    public class JavaVM : IDisposable
    {
        [DllImport("jvm.dll", CallingConvention = CallingConvention.Winapi)]
        internal static extern int JNI_CreateJavaVM(out IntPtr pVM, out IntPtr pEnv, IntPtr Args);

        [DllImport("jvm.dll", CallingConvention = CallingConvention.Winapi)]
        internal static extern int JNI_GetDefaultJavaVMInitArgs(IntPtr args);

        [DllImport("jvm.dll", CallingConvention = CallingConvention.Winapi)]
        internal static extern int JNI_GetCreatedJavaVMs(out IntPtr pVM, int jSize1, [Out] out int jSize2);

        public struct JavaVMDelegates
        {
            [UnmanagedFunctionPointer(CallingConvention.Winapi)]
            [SuppressUnmanagedCodeSecurity]
            internal delegate int DestroyJavaVM(IntPtr pVM);

            [UnmanagedFunctionPointer(CallingConvention.Winapi)]
            [SuppressUnmanagedCodeSecurity]
            internal delegate int AttachCurrentThread(IntPtr pVM, out IntPtr pEnv, IntPtr Args);

            [UnmanagedFunctionPointer(CallingConvention.Winapi)]
            [SuppressUnmanagedCodeSecurity]
            internal delegate int DetachCurrentThread(IntPtr pVM);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct JavaVMFunctions
        {
            public IntPtr reserved0;
            public IntPtr reserved1;
            public IntPtr reserved2;

            public IntPtr DestroyJavaVM;
            public IntPtr AttachCurrentThread;
            public IntPtr DetachCurrentThread;
            public IntPtr GetEnv;
            public IntPtr AttachCurrentThreadAsDaemon;
        }

        private JavaVMDelegates.AttachCurrentThread _attachCurrentThread;
        private JavaVMDelegates.DestroyJavaVM _destroyJavaVm;
        private JavaVMDelegates.DetachCurrentThread _detachCurrentThread;

        private IntPtr _jvm;
        private JavaVMFunctions _functions;

        public JavaVM(IntPtr pointer)
        {
            this._jvm = pointer;
            _functions = (JavaVMFunctions)Marshal.PtrToStructure(Marshal.ReadIntPtr(_jvm), typeof(JavaVMFunctions));
        }

        public static byte BooleanToByte(bool value)
        {
            return value ? (byte)1 : (byte)0;
        }

        public static void GetDelegateForFunctionPointer<T>(IntPtr ptr, ref T res)
        {
            res = (T)(object)Marshal.GetDelegateForFunctionPointer(ptr, typeof(T));
        }

        internal int AttachCurrentThread(out JavaInvoker penv, IntPtr args)
        {
            if (_attachCurrentThread == null)
            {
                GetDelegateForFunctionPointer(_functions.AttachCurrentThread, ref _attachCurrentThread);
            }
            IntPtr env;
            int result = _attachCurrentThread(_jvm, out env, args);
            penv = new JavaInvoker(env);
            return result;
        }

        public int DestroyJavaVM()
        {
            if (_destroyJavaVm == null)
            {
                GetDelegateForFunctionPointer(_functions.DestroyJavaVM, ref _destroyJavaVm);
            }
            return _destroyJavaVm.Invoke(_jvm);
        }

        public int DetachCurrentThread()
        {
            if (_detachCurrentThread == null)
            {
                GetDelegateForFunctionPointer(_functions.DetachCurrentThread, ref _detachCurrentThread);
            }
            return _detachCurrentThread.Invoke(_jvm);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~JavaVM() { Dispose(false); }

        protected virtual void Dispose(bool disposing)
        {
        }
    }

}
