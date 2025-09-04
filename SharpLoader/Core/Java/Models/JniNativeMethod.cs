using System.Runtime.InteropServices;

namespace SharpLoader.Core.Java.Models;

[StructLayout(LayoutKind.Sequential)]
public class JniNativeMethod
{
    public IntPtr name;
    public IntPtr signature;
    public IntPtr fnPtr;
}