using SharpLoader.Core.Java;
using SharpLoader.Core.Java.Utilities;

namespace SharpLoader.Core.ASM;

public class AsmEditor
{
    
    public IntPtr JvmHandle { get; private set; }
    public IntPtr PEnvHandle { get; private set; }
    public JvmTable JvmTable { get; private set; }
    public JniTable JniTable { get; private set; }
    
    public JavaHelper JavaHelper { get; private set; }
    
    public AsmEditor(IntPtr jvm)
    {
        JvmHandle = jvm;
        JvmTable = new JvmTable(jvm);
        var result = JvmTable.FunctionAttachCurrentThread()(jvm, out var pEnv, IntPtr.Zero);
        if (result == 0)
        {
            PEnvHandle = pEnv;
        }
        else
        {
            PEnvHandle = IntPtr.Zero;
        }
        
        JniTable = new JniTable(PEnvHandle);
        JavaHelper = new JavaHelper(JvmHandle, PEnvHandle);
    }

    #region Functions

    // TODO...

    #endregion
}