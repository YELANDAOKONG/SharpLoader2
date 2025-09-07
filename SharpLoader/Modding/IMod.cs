namespace SharpLoader.Modding;

public interface IMod
{
    bool Setup(IntPtr jvm, IntPtr env);
    void Initialize();
}