// InvokeTable.cs
using System;
using System.Runtime.InteropServices;
using SharpLoader.Core.Java.Models;

namespace SharpLoader.Core.Java;

public static class InvokeTable
{
    #region Functions

    // Core JVM functions
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int JniCreateJavaVmDelegate(
        out IntPtr pVm, 
        out IntPtr pEnv, 
        IntPtr vmArgs);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int JniGetDefaultJavaVMInitArgsDelegate(IntPtr vmArgs);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int JniGetCreatedJavaVMsDelegate(
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] IntPtr[] vmBuf,
        JSize bufLen,
        out JSize nVMs);

    // JavaVM function table delegates
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DestroyJavaVMDelegate(IntPtr vm);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int AttachCurrentThreadDelegate(
        IntPtr vm, 
        out IntPtr penv, 
        IntPtr args);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DetachCurrentThreadDelegate(IntPtr vm);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int GetEnvDelegate(IntPtr vm, out IntPtr env, int version);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int AttachCurrentThreadAsDaemonDelegate(
        IntPtr vm, 
        out IntPtr penv, 
        IntPtr args);

    #endregion

    // JNI version constants
    public const int JNI_VERSION_1_1 = 0x00010001;
    public const int JNI_VERSION_1_2 = 0x00010002;
    public const int JNI_VERSION_1_4 = 0x00010004;
    public const int JNI_VERSION_1_6 = 0x00010006;
    public const int JNI_VERSION_1_8 = 0x00010008;
    public const int JNI_VERSION_9 = 0x00090000;
    public const int JNI_VERSION_10 = 0x000A0000;
}