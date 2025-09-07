using System.Runtime.InteropServices;
using SharpLoader.Utilities;

namespace SharpLoader.Core.Modding;

public class ModuleManager
{
    public static ModuleManager? Instance { get; set; }
    
    public LoggerService Logger { get; private set; }

    public ModuleManager(LoggerService logger)
    {
        Logger = logger;
    }
    
    // TODO...
    
    
    public bool ShouldModifyClassDynamic(IntPtr env, IntPtr clazz, string className)
    {
        return false;
    }

    public byte[]? ModifyClassFileDynamic(IntPtr env, IntPtr clazz, string className, byte[] classfileBuffer)
    {
        return null;
    }

    #region Native Methods
    
    public static bool ShouldModifyClass(IntPtr env, IntPtr clazz, string className)
    {
        if (Instance == null) return false;
        return Instance.ShouldModifyClassDynamic(env, clazz, className);
    }

    public static byte[]? ModifyClassFile(IntPtr env, IntPtr clazz, string className, byte[] classfileBuffer)
    {
        if (Instance == null) return null;
        return Instance.ModifyClassFileDynamic(env, clazz, className, classfileBuffer);
    }
    
    #endregion
    
}