using System.Runtime.InteropServices;
using SharpLoader.Core.Java.Models;

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

    #endregion
}