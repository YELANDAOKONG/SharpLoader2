using System;
using System.Runtime.InteropServices;

namespace SharpLoader.Core.Java;

public static class InvokeTable
{
    #region Functions

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int JniCreateJavaVm(
        out IntPtr pVm, 
        out IntPtr pEnv, 
        IntPtr vmArgs);

    #endregion
}