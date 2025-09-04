using System.Runtime.InteropServices;

namespace SharpLoader.Core.Java.Models;

[StructLayout(LayoutKind.Sequential)]
public struct JavaVmInitArgs
{
    public int version;
    public int nOptions;
    public IntPtr options;  // Pointer to JavaVmOption array
    [MarshalAs(UnmanagedType.Bool)]
    public bool ignoreUnrecognized;
}