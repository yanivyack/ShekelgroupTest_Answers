using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ENV.Java
{
    public class JavaInvoker : IDisposable
    {
        private IntPtr pointer;
        private JavaInvokerFunctions functions;
        private JavaVM javaVM;

        internal JavaInvoker(IntPtr pointer)
        {
            this.pointer = pointer;
            functions = (JavaInvokerFunctions)Marshal.PtrToStructure(Marshal.ReadIntPtr(pointer), typeof(JavaInvokerFunctions));
        }

        public IntPtr FindClass(string name)
        {
            if (findClass == null)
                JavaVM.GetDelegateForFunctionPointer(functions.FindClass, ref findClass);
            return findClass.Invoke(pointer, name);
        }

        internal IntPtr GetObjectClass(IntPtr obj)
        {
            if (getObjectClass == null)
                JavaVM.GetDelegateForFunctionPointer(functions.GetObjectClass, ref getObjectClass);
            return getObjectClass.Invoke(pointer, obj);
        }

        public IntPtr GetMethodId(IntPtr jniClass, string name, string sig)
        {
            if (getMethodId == null)
                JavaVM.GetDelegateForFunctionPointer(functions.GetMethodID, ref getMethodId);

            return getMethodId.Invoke(pointer, jniClass, name, sig);
        }

        public IntPtr GetFieldID(IntPtr jniClass, string name, string sig)
        {
            if (getFieldID == null)
                JavaVM.GetDelegateForFunctionPointer(functions.GetFieldID, ref getFieldID);
            return getFieldID.Invoke(pointer, jniClass, name, sig);
        }

        public IntPtr GetStaticFieldID(IntPtr classHandle, string name, string sig)
        {
            if (getStaticFieldID == null)
                JavaVM.GetDelegateForFunctionPointer(functions.GetStaticFieldID, ref getStaticFieldID);
            return getStaticFieldID(pointer, classHandle, name, sig);
        }

        public IntPtr GetStaticMethodID(IntPtr jniClass, string name, string sig)
        {
            if (getStaticMethodId == null)
                JavaVM.GetDelegateForFunctionPointer(functions.GetStaticMethodID, ref getStaticMethodId);
            return getStaticMethodId.Invoke(pointer, jniClass, name, sig);
        }

        public IntPtr NewObject(IntPtr classHandle, IntPtr methodID, params JValue[] args)
        {
            if (newObject == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.NewObjectA, ref newObject);
            }

            IntPtr res = newObject(pointer, classHandle, methodID, args);
            return res;
        }

        public IntPtr CallObjectMethod(IntPtr obj, IntPtr methodID, params JValue[] args)
        {
            if (callObjectMethod == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.CallObjectMethodA, ref callObjectMethod);
            }
            IntPtr res = callObjectMethod(pointer, obj, methodID, args);
            return res;
        }

        public bool CallBooleanMethod(IntPtr obj, IntPtr methodId, params JValue[] args)
        {
            if (callBooleanMethod == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.CallBooleanMethodA, ref callBooleanMethod);
            }
            bool res = callBooleanMethod(pointer, obj, methodId, args) != 0;
            return res;
        }

        public int CallIntMethod(IntPtr obj, IntPtr methodId, params JValue[] args)
        {
            if (callIntMethod == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.CallIntMethodA, ref callIntMethod);
            }
            int res = callIntMethod(pointer, obj, methodId, args);
            return res;
        }

        public short CallShortMethod(IntPtr obj, IntPtr methodId, params JValue[] args)
        {
            if (callShortMethod == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.CallShortMethodA, ref callShortMethod);
            }
            short res = callShortMethod(pointer, obj, methodId, args);
            return res;
        }

        public long CallLongMethod(IntPtr obj, IntPtr methodId, params JValue[] args)
        {
            if (callLongMethod == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.CallLongMethodA, ref callLongMethod);
            }
            long res = callLongMethod(pointer, obj, methodId, args);
            return res;
        }

        public byte CallByteMethod(IntPtr obj, IntPtr methodId, params JValue[] args)
        {
            if (callByteMethod == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.CallByteMethodA, ref callByteMethod);
            }
            byte res = callByteMethod(pointer, obj, methodId, args);
            return res;
        }

        public double CallDoubleMethod(IntPtr obj, IntPtr methodId, params JValue[] args)
        {
            if (callDoubleMethod == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.CallDoubleMethodA, ref callDoubleMethod);
            }
            double res = callDoubleMethod(pointer, obj, methodId, args);
            return res;
        }

        public float CallFloatMethod(IntPtr obj, IntPtr methodId, params JValue[] args)
        {
            if (callFloatMethod == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.CallFloatMethodA, ref callFloatMethod);
            }
            float res = callFloatMethod(pointer, obj, methodId, args);
            return res;
        }

        public char CallCharMethod(IntPtr obj, IntPtr methodId, params JValue[] args)
        {
            if (callCharMethod == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.CallCharMethodA, ref callCharMethod);
            }
            var res = (char)callCharMethod(pointer, obj, methodId, args);
            return res;
        }

        public void CallVoidMethod(IntPtr obj, IntPtr methodId, params JValue[] args)
        {
            if (callVoidMethod == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.CallVoidMethodA, ref callVoidMethod);
            }
            callVoidMethod(pointer, obj, methodId, args);
            return;
        }


        #region Call Static Methods

        public void CallStaticVoidMethod(IntPtr jniClass, IntPtr methodId, params JValue[] args)
        {
            if (callStaticVoidMethod == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.CallStaticVoidMethodA, ref callStaticVoidMethod);
            }
            callStaticVoidMethod(pointer, jniClass, methodId, args);
        }

        public IntPtr CallStaticObjectMethod(IntPtr obj, IntPtr methodID, params JValue[] args)
        {
            if (callObjectMethod == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.CallStaticObjectMethodA, ref callStaticObjectMethod);
            }
            IntPtr res = callStaticObjectMethod(pointer, obj, methodID, args);
            return res;
        }

        public int CallStaticIntMethod(IntPtr jniClass, IntPtr MethodId, params JValue[] args)
        {
            if (callStaticIntMethod == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.CallStaticIntMethodA, ref callStaticIntMethod);
            }
            int res = callStaticIntMethod(pointer, jniClass, MethodId, args);
            return res;
        }

        public long CallStaticLongMethod(IntPtr jniClass, IntPtr MethodId, params JValue[] args)
        {
            if (callStaticLongMethod == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.CallStaticLongMethodA, ref callStaticLongMethod);
            }
            long res = callStaticLongMethod(pointer, jniClass, MethodId, args);
            return res;
        }

        public double CallStaticDoubleMethod(IntPtr jniClass, IntPtr MethodId, params JValue[] args)
        {
            if (callStaticDoubleMethod == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.CallStaticDoubleMethodA, ref callStaticDoubleMethod);
            }
            double res = callStaticDoubleMethod(pointer, jniClass, MethodId, args);
            return res;
        }

        public float CallStaticFloatMethod(IntPtr jniClass, IntPtr MethodId, params JValue[] args)
        {
            if (callStaticFloatMethod == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.CallStaticFloatMethodA, ref callStaticFloatMethod);
            }
            float res = callStaticFloatMethod(pointer, jniClass, MethodId, args);
            return res;
        }

        public short CallStaticShortMethod(IntPtr jniClass, IntPtr MethodId, params JValue[] args)
        {
            if (callStaticShortMethod == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.CallStaticShortMethodA, ref callStaticShortMethod);
            }
            short res = callStaticShortMethod(pointer, jniClass, MethodId, args);
            return res;
        }

        public char CallStaticCharMethod(IntPtr jniClass, IntPtr MethodId, params JValue[] args)
        {
            if (callStaticCharMethod == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.CallStaticCharMethodA, ref callStaticCharMethod);
            }
            var res = (char)callStaticCharMethod(pointer, jniClass, MethodId, args);
            return res;
        }

        public bool CallStaticBooleanMethod(IntPtr jniClass, IntPtr MethodId, params JValue[] args)
        {
            if (callStaticBooleanMethod == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.CallStaticBooleanMethodA, ref callStaticBooleanMethod);
            }
            bool res = callStaticBooleanMethod(pointer, jniClass, MethodId, args) != 0;
            return res;
        }

        public byte CallStaticByteMethod(IntPtr jniClass, IntPtr MethodId, params JValue[] args)
        {
            if (callStaticByteMethod == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.CallStaticByteMethodA, ref callStaticByteMethod);
            }
            byte res = callStaticByteMethod(pointer, jniClass, MethodId, args);
            return res;
        }

        #endregion

        #region getters instance

        public IntPtr GetObjectField(IntPtr obj, IntPtr fieldID)
        {
            if (getObjectField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.GetObjectField, ref getObjectField);
            }
            IntPtr res = getObjectField(pointer, obj, fieldID);
            return res;
        }

        public bool GetBooleanField(IntPtr obj, IntPtr fieldID)
        {
            if (getBooleanField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.GetBooleanField, ref getBooleanField);
            }
            bool res = getBooleanField(pointer, obj, fieldID) != 0;
            return res;
        }

        public byte GetByteField(IntPtr obj, IntPtr fieldID)
        {
            if (getByteField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.GetByteField, ref getByteField);
            }
            byte res = getByteField(pointer, obj, fieldID);
            return res;
        }

        public short GetShortField(IntPtr obj, IntPtr fieldID)
        {
            if (getShortField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.GetShortField, ref getShortField);
            }
            short res = getShortField(pointer, obj, fieldID);
            return res;
        }

        public long GetLongField(IntPtr obj, IntPtr fieldID)
        {
            if (getLongField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.GetLongField, ref getLongField);
            }
            long res = getLongField(pointer, obj, fieldID);
            return res;
        }

        public int GetIntField(IntPtr obj, IntPtr fieldID)
        {
            if (getIntField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.GetIntField, ref getIntField);
            }
            int res = getIntField(pointer, obj, fieldID);
            return res;
        }

        public double GetDoubleField(IntPtr obj, IntPtr fieldID)
        {
            if (getDoubleField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.GetDoubleField, ref getDoubleField);
            }
            double res = getDoubleField(pointer, obj, fieldID);
            return res;
        }

        public float GetFloatField(IntPtr obj, IntPtr fieldID)
        {
            if (getFloatField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.GetFloatField, ref getFloatField);
            }
            float res = getFloatField(pointer, obj, fieldID);
            return res;
        }

        public char GetCharField(IntPtr obj, IntPtr fieldID)
        {
            if (getCharField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.GetCharField, ref getCharField);
            }
            var res = (char)getCharField(pointer, obj, fieldID);
            return res;
        }

        #endregion

        #region getters static

        public IntPtr GetStaticObjectField(IntPtr clazz, IntPtr fieldID)
        {
            if (getStaticObjectField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.GetStaticObjectField, ref getStaticObjectField);
            }
            IntPtr res = getStaticObjectField(pointer, clazz, fieldID);
            return res;
        }


        public bool GetStaticBooleanField(IntPtr clazz, IntPtr fieldID)
        {
            if (getStaticBooleanField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.GetStaticBooleanField, ref getStaticBooleanField);
            }
            bool res = getStaticBooleanField(pointer, clazz, fieldID) != 0;
            return res;
        }

        public byte GetStaticByteField(IntPtr classHandle, IntPtr fieldID)
        {
            if (getStaticByteField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.GetStaticByteField, ref getStaticByteField);
            }
            byte res = getStaticByteField(pointer, classHandle, fieldID);
            return res;
        }

        public short GetStaticShortField(IntPtr classHandle, IntPtr fieldID)
        {
            if (getStaticShortField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.GetStaticShortField, ref getStaticShortField);
            }
            short res = getStaticShortField(pointer, classHandle, fieldID);
            return res;
        }

        public long GetStaticLongField(IntPtr classHandle, IntPtr fieldID)
        {
            if (getStaticLongField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.GetStaticLongField, ref getStaticLongField);
            }
            long res = getStaticLongField(pointer, classHandle, fieldID);
            return res;
        }

        public int GetStaticIntField(IntPtr classHandle, IntPtr fieldID)
        {
            if (getStaticIntField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.GetStaticIntField, ref getStaticIntField);
            }
            int res = getStaticIntField(pointer, classHandle, fieldID);
            return res;
        }

        public double GetStaticDoubleField(IntPtr classHandle, IntPtr fieldID)
        {
            if (getStaticDoubleField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.GetStaticDoubleField, ref getStaticDoubleField);
            }
            double res = getStaticDoubleField(pointer, classHandle, fieldID);
            return res;
        }

        public void SetByteArrayRegion(IntPtr handle, int v, int length, byte[] buf)
        {
            if (setByteArrayRegion == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.SetByteArrayRegion, ref setByteArrayRegion);
            }
            setByteArrayRegion(pointer, handle, v, length, buf);
        }

        public float GetStaticFloatField(IntPtr classHandle, IntPtr fieldID)
        {
            if (getStaticFloatField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.GetStaticFloatField, ref getStaticFloatField);
            }
            float res = getStaticFloatField(pointer, classHandle, fieldID);
            return res;
        }

        internal void GetByteArrayRegion(IntPtr handle, int v, int length, byte[] buf)
        {
            if (getByteArrayRegion == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.GetByteArrayRegion, ref getByteArrayRegion);
            }
            getByteArrayRegion(pointer, handle, v, length, buf);
        }

        public char GetStaticCharField(IntPtr classHandle, IntPtr fieldID)
        {
            if (getStaticCharField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.GetStaticCharField, ref getStaticCharField);
            }
            var res = (char)getStaticCharField(pointer, classHandle, fieldID);
            return res;
        }

        #endregion

        #region setters instance

        internal void SetObjectField(IntPtr obj, IntPtr fieldID, IntPtr value)
        {
            if (setObjectField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.SetObjectField, ref setObjectField);
            }
            setObjectField(pointer, obj, fieldID, value);
        }

        internal void SetIntField(IntPtr obj, IntPtr fieldID, int value)
        {
            if (setIntField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.SetIntField, ref setIntField);
            }
            setIntField(pointer, obj, fieldID, value);
        }

        internal void SetBooleanField(IntPtr obj, IntPtr fieldID, bool value)
        {
            if (setBooleanField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.SetBooleanField, ref setBooleanField);
            }
            setBooleanField(pointer, obj, fieldID, JavaVM.BooleanToByte(value));
        }

        internal void SetByteField(IntPtr obj, IntPtr fieldID, byte value)
        {
            if (setByteField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.SetByteField, ref setByteField);
            }
            setByteField(pointer, obj, fieldID, value);
        }

        internal void SetCharField(IntPtr obj, IntPtr fieldID, char value)
        {
            if (setCharField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.SetCharField, ref setCharField);
            }
            setCharField(pointer, obj, fieldID, value);
        }

        internal void SetShortField(IntPtr obj, IntPtr fieldID, short value)
        {
            if (setShortField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.SetShortField, ref setShortField);
            }
            setShortField(pointer, obj, fieldID, value);
        }

        internal void SetLongField(IntPtr obj, IntPtr fieldID, long value)
        {
            if (setLongField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.SetLongField, ref setLongField);
            }
            setLongField(pointer, obj, fieldID, value);
        }

        internal void SetFloatField(IntPtr obj, IntPtr fieldID, float value)
        {
            if (setFloatField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.SetFloatField, ref setFloatField);
            }
            setFloatField(pointer, obj, fieldID, value);
        }

        internal void SetDoubleField(IntPtr obj, IntPtr fieldID, double value)
        {
            if (setDoubleField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.SetDoubleField, ref setDoubleField);
            }
            setDoubleField(pointer, obj, fieldID, value);
        }

        # endregion

        #region setters static

        internal void SetStaticObjectField(IntPtr classHandle, IntPtr fieldID, IntPtr value)
        {
            if (setStaticObjectField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.SetStaticObjectField, ref setStaticObjectField);
            }
            setStaticObjectField(pointer, classHandle, fieldID, value);
        }

        internal void SetStaticIntField(IntPtr classHandle, IntPtr fieldID, int value)
        {
            if (setStaticIntField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.SetStaticIntField, ref setStaticIntField);
            }
            setStaticIntField(pointer, classHandle, fieldID, value);
        }

        internal void SetStaticBooleanField(IntPtr classHandle, IntPtr fieldID, bool value)
        {
            if (setStaticBooleanField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.SetStaticBooleanField, ref setStaticBooleanField);
            }
            setStaticBooleanField(pointer, classHandle, fieldID, JavaVM.BooleanToByte(value));
        }

        internal void SetStaticByteField(IntPtr classHandle, IntPtr fieldID, byte value)
        {
            if (setStaticByteField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.SetStaticByteField, ref setStaticByteField);
            }
            setStaticByteField(pointer, classHandle, fieldID, value);
        }

        internal void SetStaticCharField(IntPtr classHandle, IntPtr fieldID, char value)
        {
            if (setStaticCharField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.SetStaticCharField, ref setStaticCharField);
            }
            setStaticCharField(pointer, classHandle, fieldID, value);
        }

        internal void SetStaticShortField(IntPtr classHandle, IntPtr fieldID, short value)
        {
            if (setStaticShortField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.SetStaticShortField, ref setStaticShortField);
            }
            setStaticShortField(pointer, classHandle, fieldID, value);
        }

        internal void SetStaticLongField(IntPtr classHandle, IntPtr fieldID, long value)
        {
            if (setStaticLongField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.SetStaticLongField, ref setStaticLongField);
            }
            setStaticLongField(pointer, classHandle, fieldID, value);
        }

        internal void SetStaticFloatField(IntPtr classHandle, IntPtr fieldID, float value)
        {
            if (setStaticFloatField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.SetStaticFloatField, ref setStaticFloatField);
            }
            setStaticFloatField(pointer, classHandle, fieldID, value);
        }

        internal void SetStaticDoubleField(IntPtr classHandle, IntPtr fieldID, double value)
        {
            if (setStaticDoubleField == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.SetStaticDoubleField, ref setStaticDoubleField);
            }
            setStaticDoubleField(pointer, classHandle, fieldID, value);
        }

        #endregion

        #region string methods

        public IntPtr NewString(String unicode, int len)
        {
            if (newString == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.NewString, ref newString);
            }
            IntPtr res = newString(pointer, unicode, len);
            return res;
        }

        public IntPtr NewStringUFT(IntPtr UFT)
        {
            if (newStringUTF == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.NewStringUTF, ref newStringUTF);
            }
            IntPtr res = newStringUTF(pointer, UFT);
            return res;
        }

        internal void ReleaseStringChars(IntPtr JStr, IntPtr chars)
        {
            if (releaseStringChars == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.ReleaseStringChars, ref releaseStringChars);
            }
            releaseStringChars(pointer, JStr, chars);
        }

        internal string JStringToString(IntPtr JStr)
        {
            if (JStr != null)
            {
                if (getStringChars == null)
                {
                    JavaVM.GetDelegateForFunctionPointer(functions.GetStringChars, ref getStringChars);
                }
                var b = new IntPtr();
                var chars = getStringChars(pointer, JStr, b);
                string result = Marshal.PtrToStringUni(chars);
                ReleaseStringChars(JStr, chars);
                return result;
            }
            else return null;
        }

        #endregion

        public int GetArrayLength(IntPtr handle)
        {
            if (getArrayLength == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.GetArrayLength, ref getArrayLength);
            }
            var res = (int)getArrayLength(pointer, handle);
            return res;
        }

        internal void DeleteGlobalRef(IntPtr objectHandle)
        {
            if (deleteGlobalRef == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.DeleteGlobalRef, ref deleteGlobalRef);
            }
            if (objectHandle != null)
            {
                deleteGlobalRef(pointer, objectHandle);
            }
        }

        internal void DeleteLocalRef(IntPtr objectHandle)
        {
            if (deleteLocalRef == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.DeleteLocalRef, ref deleteLocalRef);
            }
            if (objectHandle != null)
            {
                deleteLocalRef(pointer, objectHandle);
            }
        }

        public string CatchJavaException()
        {
            if (exceptionOccurred == null)
                JavaVM.GetDelegateForFunctionPointer(functions.ExceptionOccurred, ref exceptionOccurred);
            IntPtr occurred = exceptionOccurred(pointer);
            if (occurred != IntPtr.Zero)
            {
                if (exceptionClear == null)
                {
                    JavaVM.GetDelegateForFunctionPointer(functions.ExceptionClear, ref exceptionClear);
                }
                exceptionClear(pointer);
                IntPtr ExceptionClass = this.GetObjectClass(occurred);
                IntPtr mid = GetMethodId(ExceptionClass, "toString", "()Ljava/lang/String;");
                IntPtr jstr = CallObjectMethod(occurred, mid, new JValue() { });

                return JStringToString(jstr);
            }
            return "";
        }

        public IntPtr NewByteArray(int len)
        {
            if (newByteArray == null)
            {
                JavaVM.GetDelegateForFunctionPointer(functions.NewByteArray, ref newByteArray);
            }
            IntPtr res = newByteArray(pointer, len);
            return res;
        }


        private JavaInvokerDelegates.CallBooleanMethod callBooleanMethod;
        private JavaInvokerDelegates.CallByteMethod callByteMethod;
        private JavaInvokerDelegates.CallCharMethod callCharMethod;
        private JavaInvokerDelegates.CallDoubleMethod callDoubleMethod;
        private JavaInvokerDelegates.CallFloatMethod callFloatMethod;
        private JavaInvokerDelegates.CallIntMethod callIntMethod;
        private JavaInvokerDelegates.CallLongMethod callLongMethod;
        private JavaInvokerDelegates.CallVoidMethod callVoidMethod;

        private JavaInvokerDelegates.CallObjectMethod callObjectMethod;
        private JavaInvokerDelegates.CallShortMethod callShortMethod;
        private JavaInvokerDelegates.CallStaticBooleanMethod callStaticBooleanMethod;
        private JavaInvokerDelegates.CallStaticByteMethod callStaticByteMethod;
        private JavaInvokerDelegates.CallStaticCharMethod callStaticCharMethod;
        private JavaInvokerDelegates.CallStaticDoubleMethod callStaticDoubleMethod;
        private JavaInvokerDelegates.CallStaticFloatMethod callStaticFloatMethod;
        private JavaInvokerDelegates.CallStaticIntMethod callStaticIntMethod;
        private JavaInvokerDelegates.CallStaticLongMethod callStaticLongMethod;
        private JavaInvokerDelegates.CallStaticObjectMethod callStaticObjectMethod;
        private JavaInvokerDelegates.CallStaticShortMethod callStaticShortMethod;
        private JavaInvokerDelegates.CallStaticVoidMethod callStaticVoidMethod;

        private JavaInvokerDelegates.DeleteGlobalRef deleteGlobalRef;
        private JavaInvokerDelegates.DeleteLocalRef deleteLocalRef;
        private JavaInvokerDelegates.ExceptionClear exceptionClear;
        private JavaInvokerDelegates.ExceptionOccurred exceptionOccurred;
        private JavaInvokerDelegates.FindClass findClass;
        private JavaInvokerDelegates.GetBooleanField getBooleanField;
        private JavaInvokerDelegates.GetByteField getByteField;
        private JavaInvokerDelegates.GetCharField getCharField;
        private JavaInvokerDelegates.GetDoubleField getDoubleField;
        private JavaInvokerDelegates.GetFieldID getFieldID;
        private JavaInvokerDelegates.GetFloatField getFloatField;
        private JavaInvokerDelegates.GetIntField getIntField;
        private JavaInvokerDelegates.GetJavaVM getJavaVM;
        private JavaInvokerDelegates.GetLongField getLongField;
        private JavaInvokerDelegates.GetMethodId getMethodId;
        private JavaInvokerDelegates.GetObjectClass getObjectClass;
        private JavaInvokerDelegates.GetObjectField getObjectField;
        private JavaInvokerDelegates.GetShortField getShortField;
        private JavaInvokerDelegates.GetStaticBooleanField getStaticBooleanField;
        private JavaInvokerDelegates.GetStaticByteField getStaticByteField;
        private JavaInvokerDelegates.GetStaticCharField getStaticCharField;
        private JavaInvokerDelegates.GetStaticDoubleField getStaticDoubleField;
        private JavaInvokerDelegates.GetStaticFieldID getStaticFieldID;
        private JavaInvokerDelegates.GetStaticFloatField getStaticFloatField;
        private JavaInvokerDelegates.GetStaticIntField getStaticIntField;
        private JavaInvokerDelegates.GetStaticLongField getStaticLongField;
        private JavaInvokerDelegates.GetStaticMethodId getStaticMethodId;
        private JavaInvokerDelegates.GetStaticObjectField getStaticObjectField;
        private JavaInvokerDelegates.GetStaticShortField getStaticShortField;
        private JavaInvokerDelegates.GetStringChars getStringChars;
        private JavaInvokerDelegates.NewObject newObject;
        private JavaInvokerDelegates.NewString newString;
        private JavaInvokerDelegates.NewByteArray newByteArray;
        private JavaInvokerDelegates.NewStringUTF newStringUTF;
        private JavaInvokerDelegates.ReleaseStringChars releaseStringChars;
        private JavaInvokerDelegates.SetBooleanField setBooleanField;
        private JavaInvokerDelegates.SetByteField setByteField;
        private JavaInvokerDelegates.SetCharField setCharField;
        private JavaInvokerDelegates.SetDoubleField setDoubleField;
        private JavaInvokerDelegates.SetFloatField setFloatField;
        private JavaInvokerDelegates.SetIntField setIntField;
        private JavaInvokerDelegates.SetLongField setLongField;
        private JavaInvokerDelegates.SetObjectField setObjectField;
        private JavaInvokerDelegates.SetShortField setShortField;
        private JavaInvokerDelegates.SetStaticBooleanField setStaticBooleanField;
        private JavaInvokerDelegates.SetStaticByteField setStaticByteField;
        private JavaInvokerDelegates.SetStaticCharField setStaticCharField;
        private JavaInvokerDelegates.SetStaticDoubleField setStaticDoubleField;
        private JavaInvokerDelegates.SetStaticFloatField setStaticFloatField;
        private JavaInvokerDelegates.SetStaticIntField setStaticIntField;
        private JavaInvokerDelegates.SetStaticLongField setStaticLongField;
        private JavaInvokerDelegates.SetStaticObjectField setStaticObjectField;
        private JavaInvokerDelegates.SetStaticShortField setStaticShortField;
        private JavaInvokerDelegates.GetArrayLength getArrayLength;
        private JavaInvokerDelegates.SetByteArrayRegion setByteArrayRegion;
        private JavaInvokerDelegates.GetByteArrayRegion getByteArrayRegion;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (javaVM != null)
                {
                    javaVM.Dispose();
                    javaVM = null;
                }
            }
        }
    }

    public struct JavaInvokerFunctions
    {
        public IntPtr reserved0;
        public IntPtr reserved1;
        public IntPtr reserved2;
        public IntPtr reserved3;
        public IntPtr GetVersion;
        public IntPtr DefineClass;
        public IntPtr FindClass;
        // Reflection support
        public IntPtr FromReflectedMethod;
        public IntPtr FromReflectedField;
        public IntPtr ToReflectedMethod;

        public IntPtr GetSuperclass;
        public IntPtr IsAssignableFrom;
        // Reflection support
        public IntPtr ToReflectedField;

        public IntPtr Throw;
        public IntPtr ThrowNew;
        public IntPtr ExceptionOccurred;
        public IntPtr ExceptionDescribe;
        public IntPtr ExceptionClear;
        public IntPtr FatalError;

        // Local Reference Management
        public IntPtr PushLocalFrame;
        public IntPtr PopLocalFrame;

        public IntPtr NewGlobalRef;
        public IntPtr DeleteGlobalRef;
        public IntPtr DeleteLocalRef;
        public IntPtr IsSameObject;
        public IntPtr NewLocalRef;
        public IntPtr EnsureLocalCapacity;
        public IntPtr AllocObject;

        public IntPtr NewObject;
        public IntPtr NewObjectV;
        public IntPtr NewObjectA;

        public IntPtr GetObjectClass;
        public IntPtr IsInstanceOf;
        public IntPtr GetMethodID;
        public IntPtr CallObjectMethod;
        public IntPtr CallObjectMethodV;
        public IntPtr CallObjectMethodA;
        public IntPtr CallBooleanMethod;
        public IntPtr CallBooleanMethodV;
        public IntPtr CallBooleanMethodA;
        public IntPtr CallByteMethod;
        public IntPtr CallByteMethodV;
        public IntPtr CallByteMethodA;
        public IntPtr CallCharMethod;
        public IntPtr CallCharMethodV;
        public IntPtr CallCharMethodA;
        public IntPtr CallShortMethod;
        public IntPtr CallShortMethodV;
        public IntPtr CallShortMethodA;
        public IntPtr CallIntMethod;
        public IntPtr CallIntMethodV;
        public IntPtr CallIntMethodA;
        public IntPtr CallLongMethod;
        public IntPtr CallLongMethodV;
        public IntPtr CallLongMethodA;
        public IntPtr CallFloatMethod;
        public IntPtr CallFloatMethodV;
        public IntPtr CallFloatMethodA;
        public IntPtr CallDoubleMethod;
        public IntPtr CallDoubleMethodV;
        public IntPtr CallDoubleMethodA;
        public IntPtr CallVoidMethod;
        public IntPtr CallVoidMethodV;
        public IntPtr CallVoidMethodA;
        public IntPtr CallNonvirtualObjectMethod;
        public IntPtr CallNonvirtualObjectMethodV;
        public IntPtr CallNonvirtualObjectMethodA;
        public IntPtr CallNonvirtualBooleanMethod;
        public IntPtr CallNonvirtualBooleanMethodV;
        public IntPtr CallNonvirtualBooleanMethodA;
        public IntPtr CallNonvirtualByteMethod;
        public IntPtr CallNonvirtualByteMethodV;
        public IntPtr CallNonvirtualByteMethodA;
        public IntPtr CallNonvirtualCharMethod;
        public IntPtr CallNonvirtualCharMethodV;
        public IntPtr CallNonvirtualCharMethodA;
        public IntPtr CallNonvirtualShortMethod;
        public IntPtr CallNonvirtualShortMethodV;
        public IntPtr CallNonvirtualShortMethodA;
        public IntPtr CallNonvirtualIntMethod;
        public IntPtr CallNonvirtualIntMethodV;
        public IntPtr CallNonvirtualIntMethodA;
        public IntPtr CallNonvirtualLongMethod;
        public IntPtr CallNonvirtualLongMethodV;
        public IntPtr CallNonvirtualLongMethodA;
        public IntPtr CallNonvirtualFloatMethod;
        public IntPtr CallNonvirtualFloatMethodV;
        public IntPtr CallNonvirtualFloatMethodA;
        public IntPtr CallNonvirtualDoubleMethod;
        public IntPtr CallNonvirtualDoubleMethodV;
        public IntPtr CallNonvirtualDoubleMethodA;
        public IntPtr CallNonvirtualVoidMethod;
        public IntPtr CallNonvirtualVoidMethodV;
        public IntPtr CallNonvirtualVoidMethodA;
        public IntPtr GetFieldID;
        public IntPtr GetObjectField;
        public IntPtr GetBooleanField;
        public IntPtr GetByteField;
        public IntPtr GetCharField;
        public IntPtr GetShortField;
        public IntPtr GetIntField;
        public IntPtr GetLongField;
        public IntPtr GetFloatField;
        public IntPtr GetDoubleField;
        public IntPtr SetObjectField;
        public IntPtr SetBooleanField;
        public IntPtr SetByteField;
        public IntPtr SetCharField;
        public IntPtr SetShortField;
        public IntPtr SetIntField;
        public IntPtr SetLongField;
        public IntPtr SetFloatField;
        public IntPtr SetDoubleField;
        public IntPtr GetStaticMethodID;
        public IntPtr CallStaticObjectMethod;
        public IntPtr CallStaticObjectMethodV;
        public IntPtr CallStaticObjectMethodA;
        public IntPtr CallStaticBooleanMethod;
        public IntPtr CallStaticBooleanMethodV;
        public IntPtr CallStaticBooleanMethodA;
        public IntPtr CallStaticByteMethod;
        public IntPtr CallStaticByteMethodV;
        public IntPtr CallStaticByteMethodA;
        public IntPtr CallStaticCharMethod;
        public IntPtr CallStaticCharMethodV;
        public IntPtr CallStaticCharMethodA;
        public IntPtr CallStaticShortMethod;
        public IntPtr CallStaticShortMethodV;
        public IntPtr CallStaticShortMethodA;
        public IntPtr CallStaticIntMethod;
        public IntPtr CallStaticIntMethodV;
        public IntPtr CallStaticIntMethodA;
        public IntPtr CallStaticLongMethod;
        public IntPtr CallStaticLongMethodV;
        public IntPtr CallStaticLongMethodA;
        public IntPtr CallStaticFloatMethod;
        public IntPtr CallStaticFloatMethodV;
        public IntPtr CallStaticFloatMethodA;
        public IntPtr CallStaticDoubleMethod;
        public IntPtr CallStaticDoubleMethodV;
        public IntPtr CallStaticDoubleMethodA;
        public IntPtr CallStaticVoidMethod;
        public IntPtr CallStaticVoidMethodV;
        public IntPtr CallStaticVoidMethodA;
        public IntPtr GetStaticFieldID;
        public IntPtr GetStaticObjectField;
        public IntPtr GetStaticBooleanField;
        public IntPtr GetStaticByteField;
        public IntPtr GetStaticCharField;
        public IntPtr GetStaticShortField;
        public IntPtr GetStaticIntField;
        public IntPtr GetStaticLongField;
        public IntPtr GetStaticFloatField;
        public IntPtr GetStaticDoubleField;
        public IntPtr SetStaticObjectField;
        public IntPtr SetStaticBooleanField;
        public IntPtr SetStaticByteField;
        public IntPtr SetStaticCharField;
        public IntPtr SetStaticShortField;
        public IntPtr SetStaticIntField;
        public IntPtr SetStaticLongField;
        public IntPtr SetStaticFloatField;
        public IntPtr SetStaticDoubleField;
        public IntPtr NewString;
        public IntPtr GetStringLength;
        public IntPtr GetStringChars;
        public IntPtr ReleaseStringChars;
        public IntPtr NewStringUTF;
        public IntPtr GetStringUTFLength;
        public IntPtr GetStringUTFChars;
        public IntPtr ReleaseStringUTFChars;
        public IntPtr GetArrayLength;
        public IntPtr NewObjectArray;
        public IntPtr GetObjectArrayElement;
        public IntPtr SetObjectArrayElement;
        public IntPtr NewBooleanArray;
        public IntPtr NewByteArray;
        public IntPtr NewCharArray;
        public IntPtr NewShortArray;
        public IntPtr NewIntArray;
        public IntPtr NewLongArray;
        public IntPtr NewFloatArray;
        public IntPtr NewDoubleArray;
        public IntPtr GetBooleanArrayElements;
        public IntPtr GetByteArrayElements;
        public IntPtr GetCharArrayElements;
        public IntPtr GetShortArrayElements;
        public IntPtr GetIntArrayElements;
        public IntPtr GetLongArrayElements;
        public IntPtr GetFloatArrayElements;
        public IntPtr GetDoubleArrayElements;
        public IntPtr ReleaseBooleanArrayElements;
        public IntPtr ReleaseByteArrayElements;
        public IntPtr ReleaseCharArrayElements;
        public IntPtr ReleaseShortArrayElements;
        public IntPtr ReleaseIntArrayElements;
        public IntPtr ReleaseLongArrayElements;
        public IntPtr ReleaseFloatArrayElements;
        public IntPtr ReleaseDoubleArrayElements;
        public IntPtr GetBooleanArrayRegion;
        public IntPtr GetByteArrayRegion;
        public IntPtr GetCharArrayRegion;
        public IntPtr GetShortArrayRegion;
        public IntPtr GetIntArrayRegion;
        public IntPtr GetLongArrayRegion;
        public IntPtr GetFloatArrayRegion;
        public IntPtr GetDoubleArrayRegion;
        public IntPtr SetBooleanArrayRegion;
        public IntPtr SetByteArrayRegion;
        public IntPtr SetCharArrayRegion;
        public IntPtr SetShortArrayRegion;
        public IntPtr SetIntArrayRegion;
        public IntPtr SetLongArrayRegion;
        public IntPtr SetFloatArrayRegion;
        public IntPtr SetDoubleArrayRegion;
        public IntPtr RegisterNatives;
        public IntPtr UnregisterNatives;
        public IntPtr MonitorEnter;
        public IntPtr MonitorExit;
        public IntPtr GetJavaVM;
        public IntPtr GetStringRegion;
        public IntPtr GetStringUTFRegion;
        public IntPtr GetPrimitiveArrayCritical;
        public IntPtr ReleasePrimitiveArrayCritical;
        public IntPtr GetStringCritical;
        public IntPtr ReleaseStringCritical;
        public IntPtr NewWeakGlobalRef;
        public IntPtr DeleteWeakGlobalRef;
        public IntPtr ExceptionCheck;
        public IntPtr NewDirectByteBuffer;
        public IntPtr GetDirectBufferAddress;
        public IntPtr GetDirectBufferCapacity;
    }

    internal struct JavaInvokerDelegates
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate byte CallBooleanMethod(
            IntPtr EnvironmentHandle, IntPtr obj, IntPtr jMethodID, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate byte CallByteMethod(
            IntPtr EnvironmentHandle, IntPtr obj, IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate ushort CallCharMethod(
            IntPtr EnvironmentHandle, IntPtr obj, IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate double CallDoubleMethod(
            IntPtr EnvironmentHandle, IntPtr obj, IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate float CallFloatMethod(
            IntPtr EnvironmentHandle, IntPtr obj, IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate int CallIntMethod(
            IntPtr EnvironmentHandle, IntPtr obj, IntPtr jMethodID, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate long CallLongMethod(
            IntPtr EnvironmentHandle, IntPtr obj, IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr CallObjectMethod(
            IntPtr EnvironmentHandle, IntPtr obj, IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate short CallShortMethod(
            IntPtr EnvironmentHandle, IntPtr obj, IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void CallVoidMethod(
            IntPtr EnvironmentHandle, IntPtr obj, IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate byte CallNonvirtualBooleanMethod(
            IntPtr obj, IntPtr jniClass, IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate byte CallNonvirtualByteMethod(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jniClass,
                                                        IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate ushort CallNonvirtualCharMethod(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jniClass,
                                                          IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate double CallNonvirtualDoubleMethod(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jniClass,
                                                            IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate float CallNonvirtualFloatMethod(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jniClass,
                                                          IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate int CallNonvirtualIntMethod(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jniClass,
                                                      IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate long CallNonvirtualLongMethod(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jniClass,
                                                        IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr CallNonvirtualObjectMethod(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jniClass,
                                                            IntPtr jMethodId, params JValue[] args
            );

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate short CallNonvirtualShortMethod(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jniClass,
                                                          IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void CallNonvirtualVoidMethod(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jniClass,
                                                        IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate byte CallStaticBooleanMethod(IntPtr EnvironmentHandle, IntPtr jniClass,
                                                       IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate byte CallStaticByteMethod(
            IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate ushort CallStaticCharMethod(
            IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate double CallStaticDoubleMethod(
            IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate float CallStaticFloatMethod(
            IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate int CallStaticIntMethod(
            IntPtr EnvironmentHandle, IntPtr obj, IntPtr jMethodID, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate long CallStaticLongMethod(
            IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr CallStaticObjectMethod(
            IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate short CallStaticShortMethod(
            IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate int CallStaticVoidMethod(
            IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jMethodID, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        [SuppressUnmanagedCodeSecurity]
        internal delegate void DeleteGlobalRef(IntPtr EnvironmentHandle, IntPtr gref);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        [SuppressUnmanagedCodeSecurity]
        internal delegate void DeleteLocalRef(IntPtr EnvironmentHandle, IntPtr lref);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ExceptionClear(IntPtr EnvironmentHandle);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr ExceptionOccurred(IntPtr EnvironmentHandle);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr FindClass(IntPtr EnvironmentHandle, [MarshalAs(UnmanagedType.LPStr)] string name);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate byte GetBooleanField(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jFieldId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate byte GetByteField(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jFieldId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate ushort GetCharField(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jFieldId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate double GetDoubleField(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jFieldId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetFieldID(
            IntPtr EnvironmentHandle, IntPtr jniClass, [MarshalAs(UnmanagedType.LPStr)] string name,
            [MarshalAs(UnmanagedType.LPStr)] string sig);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate float GetFloatField(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jFieldId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate int GetIntField(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jFieldId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate int GetJavaVM(IntPtr EnvironmentHandle, out IntPtr vm);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate long GetLongField(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jFieldId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetMethodId(
            IntPtr EnvironmentHandle, IntPtr jniClass, [MarshalAs(UnmanagedType.LPStr)] string name,
            [MarshalAs(UnmanagedType.LPStr)] string sig);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetObjectClass(IntPtr EnvironmentHandle, IntPtr obj);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetObjectField(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jFieldId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate short GetShortField(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jFieldId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate byte GetStaticBooleanField(IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jFieldId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate byte GetStaticByteField(IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jFieldId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate ushort GetStaticCharField(IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jFieldId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate double GetStaticDoubleField(IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jFieldId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetStaticFieldID(
            IntPtr EnvironmentHandle, IntPtr jniClass, [MarshalAs(UnmanagedType.LPStr)] string name,
            [MarshalAs(UnmanagedType.LPStr)] string sig);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate float GetStaticFloatField(IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jFieldId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate int GetStaticIntField(IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jFieldId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate long GetStaticLongField(IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jFieldId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetStaticMethodId(IntPtr EnvironmentHandle, IntPtr jniClass,
                                                   [MarshalAs(UnmanagedType.LPStr)] string name,
                                                   [MarshalAs(UnmanagedType.LPStr)] string sig);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetStaticObjectField(IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jFieldId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate short GetStaticShortField(IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jFieldId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetStringChars(IntPtr EnvironmentHandle, IntPtr str, IntPtr isCopy);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate int GetStringLength(IntPtr EnvironmentHandle, IntPtr str);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetStringUTFChars(IntPtr EnvironmentHandle, IntPtr str, IntPtr isCopy);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate int GetStringUTFLength(IntPtr EnvironmentHandle, IntPtr str);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr NewObject(
            IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jMethodId, params JValue[] args);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr NewString(
            IntPtr EnvironmentHandle, [MarshalAs(UnmanagedType.LPWStr)] string unicode, int len);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr NewByteArray(IntPtr EnvironmentHandle, int len);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr NewStringUTF(IntPtr EnvironmentHandle, IntPtr utf);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ReleaseStringChars(IntPtr EnvironmentHandle, IntPtr str, IntPtr chars);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetBooleanField(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jFieldId, byte val);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetByteField(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jFieldId, byte val);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetCharField(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jFieldId, ushort val);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetDoubleField(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jFieldId, double val);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetFloatField(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jFieldId, float val);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetIntField(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jFieldId, int val);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetLongField(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jFieldId, long val);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetObjectArrayElement(IntPtr EnvironmentHandle, IntPtr array, int index, IntPtr val);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetObjectField(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jFieldId, IntPtr val);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetShortField(IntPtr EnvironmentHandle, IntPtr obj, IntPtr jFieldId, short val);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetStaticBooleanField(
            IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jFieldId, byte value);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetStaticByteField(IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jFieldId, byte value
            );

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetStaticCharField(
            IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jFieldId, ushort value);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetStaticDoubleField(
            IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jFieldId, double value);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetStaticFloatField(
            IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jFieldId, float value);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetStaticIntField(IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jFieldId, int value);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetStaticLongField(IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jFieldId, long value
            );

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetStaticObjectField(
            IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jFieldId, IntPtr value);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetStaticShortField(
            IntPtr EnvironmentHandle, IntPtr jniClass, IntPtr jFieldId, short value);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate int GetArrayLength(IntPtr EnvironmentHandle, IntPtr handle);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate int SetByteArrayRegion(IntPtr EnvironmentHandle, IntPtr handle, int v, int length, byte[] buf);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate int GetByteArrayRegion(IntPtr EnvironmentHandle, IntPtr handle, int v, int length, byte[] buf);
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct JValue
    {
        [FieldOffset(0)]
        public byte z;
        [FieldOffset(0)]
        public byte b;
        [FieldOffset(0)]
        public char c;
        [FieldOffset(0)]
        public short s;
        [FieldOffset(0)]
        public int i;
        [FieldOffset(0)]
        public long j;
        [FieldOffset(0)]
        public float f;
        [FieldOffset(0)]
        public double d;
        [FieldOffset(0)]
        public IntPtr l;
    }
}
