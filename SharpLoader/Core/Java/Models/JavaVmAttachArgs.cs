using System.Runtime.InteropServices;

namespace SharpLoader.Core.Java.Models;

[StructLayout(LayoutKind.Sequential)]
public struct JavaVmAttachArgs
{
    public int version;
    public IntPtr name;     // Thread name (UTF-8 string)
    public IntPtr group;    // Thread group (global ref)
}