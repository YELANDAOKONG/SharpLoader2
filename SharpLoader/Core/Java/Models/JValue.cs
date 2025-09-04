using System.Runtime.InteropServices;

namespace SharpLoader.Core.Java.Models;

[StructLayout(LayoutKind.Explicit)]
public class JValue
{
    [FieldOffset(0)] public bool z;
    [FieldOffset(0)] public sbyte b;
    [FieldOffset(0)] public char c;
    [FieldOffset(0)] public short s;
    [FieldOffset(0)] public int i;
    [FieldOffset(0)] public long j;
    [FieldOffset(0)] public float f;
    [FieldOffset(0)] public double d;
    [FieldOffset(0)] public IntPtr l;
}