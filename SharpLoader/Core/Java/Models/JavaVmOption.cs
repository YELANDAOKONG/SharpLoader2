using System.Runtime.InteropServices;

namespace SharpLoader.Core.Java.Models;

[StructLayout(LayoutKind.Sequential)]
public struct JavaVmOption
{
    public IntPtr optionString;  // Use Marshal.StringToCoTaskMemUTF8
    public IntPtr extraInfo;
}
