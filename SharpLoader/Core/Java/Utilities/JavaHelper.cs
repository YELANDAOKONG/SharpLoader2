using System.Runtime.InteropServices;
using SharpLoader.Core.Java.Models;
using SharpLoader.Core.Java.Models.Wrappers;

namespace SharpLoader.Core.Java.Utilities;

public class JavaHelper
{
    public IntPtr JvmHandle { get; private set; }
    public IntPtr EnvHandle { get; private set; }
    
    public JvmTable Jvm { get; private set; }
    public JniTable Env { get; private set; }

    public JavaHelper(IntPtr jvmHandle, IntPtr envHandle)
    {
        JvmHandle = jvmHandle;
        EnvHandle = envHandle;
        
        Jvm = new JvmTable(jvmHandle);
        Env = new JniTable(envHandle);
    }

    #region Functions

    public IntPtr FindClass(string className)
    {
        return Env.FunctionFindClass()(EnvHandle, className);
    }

    public IntPtr GetStaticMethodId(IntPtr clazz, string methodName, string signature)
    {
        IntPtr methodStringPtr = IntPtr.Zero;
        IntPtr signatureStringPtr = IntPtr.Zero;
        try
        {
            methodStringPtr = Marshal.StringToHGlobalAnsi(methodName);
            signatureStringPtr = Marshal.StringToHGlobalAnsi(signature);
            
            return Env.FunctionGetStaticMethodID()(EnvHandle, clazz, methodStringPtr, signatureStringPtr);
        }
        finally
        {
            if (methodStringPtr != IntPtr.Zero) Marshal.FreeHGlobal(methodStringPtr);
            if (signatureStringPtr != IntPtr.Zero) Marshal.FreeHGlobal(signatureStringPtr);
        }
    }
    
    public unsafe void CallStaticVoidMethodA(IntPtr clazz, IntPtr methodId, JValue[]? args = null)
    {
        IntPtr argsPtr = IntPtr.Zero;
        try
        {
            if (args != null)
            {
                int elementSize = Marshal.SizeOf(typeof(JValue));
                argsPtr = Marshal.AllocHGlobal(elementSize * args.Length);

                for (int i = 0; i < args.Length; i++)
                {
                    IntPtr currentPtr = (IntPtr)((byte*)argsPtr + i * elementSize);
                    Marshal.StructureToPtr(args[i], currentPtr, false);
                }
            }
            Env.FunctionCallStaticVoidMethodA()(EnvHandle, clazz, methodId, argsPtr);
        }
        finally
        {
            if (argsPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(argsPtr);
            }
        }
    }

    public IntPtr NewGlobalRef(IntPtr obj)
    {
        return Env.FunctionNewGlobalRef()(EnvHandle, obj);
    }

    public void DeleteGlobalRef(IntPtr globalRef)
    {
        Env.FunctionDeleteGlobalRef()(EnvHandle, globalRef);
    }

    public unsafe int RegisterNativeMethods(IntPtr clazz, JniNativeMethodWrapped[] methods)
    {
        int methodCount = methods.Length;
        int methodSize = Marshal.SizeOf<JniNativeMethod>();
        IntPtr methodsPtr = Marshal.AllocHGlobal(methodSize * methodCount);
        List<IntPtr> allocatedStrings = new List<IntPtr>();

        try
        {
            for (int i = 0; i < methodCount; i++)
            {
                var wrapped = methods[i];

                // 分配 Name 和 Signature 的非托管内存
                IntPtr namePtr = Marshal.StringToHGlobalAnsi(wrapped.Name);
                IntPtr sigPtr = Marshal.StringToHGlobalAnsi(wrapped.Signature);
                allocatedStrings.Add(namePtr);
                allocatedStrings.Add(sigPtr);

                // 构建结构体
                JniNativeMethod nativeMethod = new JniNativeMethod
                {
                    name = namePtr,
                    signature = sigPtr,
                    fnPtr = wrapped.FunctionPtr
                };

                // 将结构体写入非托管内存
                IntPtr currentMethodPtr = (IntPtr)((byte*)methodsPtr + i * methodSize);
                Marshal.StructureToPtr(nativeMethod, currentMethodPtr, false);
            }
            
            var registerNatives = Env.FunctionRegisterNatives();
            return registerNatives(EnvHandle, clazz, methodsPtr, methodCount);
        }
        finally
        {
            foreach (IntPtr ptr in allocatedStrings)
            {
                if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr);
            }
            if (methodsPtr != IntPtr.Zero) Marshal.FreeHGlobal(methodsPtr);
        }
    }


    #endregion
}