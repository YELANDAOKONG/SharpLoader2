using SharpLoader.Core.Modding;
using SharpLoader.Utilities;

namespace SharpLoader.Modding;

public interface IModule
{
    bool Setup(IntPtr jvm, IntPtr env);

    void Initialize(ModuleManager manager, LoggerService? logger) { }

    #region Classes

    // Low-Level Modify Class
    byte[]? ModifyClass(string className, byte[] classData)
    {
        return null;
    }

    #endregion
}