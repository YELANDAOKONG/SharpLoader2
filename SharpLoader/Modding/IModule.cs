namespace SharpLoader.Modding;

public interface IModule
{
    bool Setup(IntPtr jvm, IntPtr env);
    void Initialize();

    #region Classes

    // Low-Level Modify Class
    byte[]? ModifyClass(string className, byte[] classData)
    {
        return null;
    }

    #endregion
}