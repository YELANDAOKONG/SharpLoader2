namespace SharpLoader.Modding;

public interface IModule
{
    bool Setup(IntPtr jvm, IntPtr env);
    void Initialize();
}