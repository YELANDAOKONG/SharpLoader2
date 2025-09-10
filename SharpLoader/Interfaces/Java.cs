using SharpLoader.Core.Java;

namespace SharpLoader.Interfaces;

public class Java
{
    public InvokeHelper Helper { get; private set; }
    public IntPtr JvmHandle { get; private set; }
    public JvmTable JvmTable { get; private set; }

    public Java(InvokeHelper helper, IntPtr gJvm)
    {
        Helper = helper;
        JvmHandle = gJvm;
        JvmTable = new JvmTable(JvmHandle);
    }

    #region Functions
    
    public IntPtr? GetEnv()
    {
        var status = AttachCurrentThread(out var ptr);
        if (status != 0x0) return null;
        return ptr;
    }

    public JniTable? GetTable()
    {
        var status = AttachCurrentThread(out var ptr);
        if (status != 0x0) return null;
        return new JniTable(ptr);
    }

    public bool CompareClassName(string className1, string className2)
    {
        if (className1.Equals(className2))
        {
            return true;
        }

        if (className1.Replace('/',  '.').Equals(className2.Replace('/', '.')))
        {
            return true;
        }
        
        if (className1.Replace('.',  '/').Equals(className2.Replace('.', '/')))
        {
            return true;
        }
        
        return false;
    }

    #endregion

    #region Functions (Thread)

    public int AttachCurrentThread(out nint localEnvPtr)
    {
        var status = AttachCurrentThread(out localEnvPtr, IntPtr.Zero);
        return status;
    }
    
    public int AttachCurrentThread(out nint localEnvPtr, IntPtr argsPtr)
    {
        var function = JvmTable.FunctionAttachCurrentThread();
        var status = function(JvmHandle, out localEnvPtr, argsPtr);
        return status;
    }

    #endregion
}