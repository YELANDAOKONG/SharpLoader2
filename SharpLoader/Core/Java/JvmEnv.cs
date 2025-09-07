using System.Runtime.InteropServices;

namespace SharpLoader.Core.Java;

[StructLayout(LayoutKind.Sequential)]
public struct JvmEnv
{
    public IntPtr _reserved0;
    public IntPtr _reserved1;
    public IntPtr _reserved2;
    
    public IntPtr DestroyJavaVM;
    public IntPtr AttachCurrentThread;
    public IntPtr DetachCurrentThread;
    public IntPtr GetEnv;
    public IntPtr AttachCurrentThreadAsDaemon;
}