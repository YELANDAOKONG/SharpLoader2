using System.Runtime.InteropServices;

namespace SharpLoader.Core.Java.Utilities;

public class JStringHelper
{
    private readonly JniTable _jniTable;
    
    public JStringHelper(IntPtr env)
    {
        _jniTable = new JniTable(env);
    }
    
    public JStringHelper(JniTable jniTable)
    {
        _jniTable = jniTable ?? throw new ArgumentNullException(nameof(jniTable));
    }
    
    public string? GetStringUtfChars(IntPtr env, IntPtr jstring)
    {
        if (env == IntPtr.Zero || jstring == IntPtr.Zero)
            return string.Empty;
        
        IntPtr stringPtr = IntPtr.Zero;
        try
        {
            var getStringUtfChars = _jniTable.FunctionGetStringUTFChars();
            stringPtr = getStringUtfChars(env, jstring, IntPtr.Zero);
            if (stringPtr == IntPtr.Zero)
                return string.Empty;
            
            var stringLength = _jniTable.FunctionGetStringUTFLength()(env, jstring);
            var result = Marshal.PtrToStringUTF8(stringPtr, stringLength);
            return result ?? string.Empty;
        }
        finally
        {
            if (stringPtr != IntPtr.Zero)
            {
                var releaseStringUtfChars = _jniTable.FunctionReleaseStringUTFChars();
                releaseStringUtfChars(env, jstring, stringPtr);
            }
        }
    }

    public void ReleaseStringUtfChars(IntPtr env, IntPtr jstring, IntPtr chars)
    {
        if (env == IntPtr.Zero || jstring == IntPtr.Zero || chars == IntPtr.Zero)
            return;
        
        var releaseStringUtfChars = _jniTable.FunctionReleaseStringUTFChars();
        releaseStringUtfChars(env, jstring, chars);
    }

    public int? GetStringUtfLength(IntPtr env, IntPtr jstring)
    {
        if (env == IntPtr.Zero || jstring == IntPtr.Zero)
            return 0;
        
        var getStringUtfLength = _jniTable.FunctionGetStringUTFLength();
        return getStringUtfLength(env, jstring);
    }
}