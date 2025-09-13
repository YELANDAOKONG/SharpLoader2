using SharpLoader.Core.Java;
using SharpLoader.Core.Modding;
using SharpLoader.Interfaces;
using SharpLoader.Utilities;

namespace SharpLoader.Modding;

public class ModuleBase : IModule
{
    
    public InvokeHelper? Invoker { get; private set; }
    public IntPtr Handle { get; private set; }
    
    public ModuleManager? Manager { get; private set; }
    public LoggerService? Logger { get; private set; }
    
    public Java? Java { get; private set; }

    public bool Setup(InvokeHelper helper, IntPtr jvm, IntPtr pEnv)
    {
        Invoker = helper;
        Handle = jvm;
        Java = new Java(helper, jvm);
        return OnSetup(helper, jvm, pEnv);
    }

    public bool OnSetup(InvokeHelper helper, IntPtr jvm, IntPtr pEnv)
    {
        return true;
    }

    public void Initialize(ModuleManager manager, LoggerService? logger)
    {
        Manager = manager;
        Logger = logger;
        
        PreInitialize();
        OnInitialize(manager, logger);
    }
    
    protected virtual void OnInitialize(ModuleManager manager, LoggerService? logger)
    {
        
    }
    
    protected virtual void PreInitialize()
    {
        
    }

    #region Classes

    // Low-Level Modify Class
    public virtual byte[]? ModifyClass(string className, byte[]? classData)
    {
        return null;
    }

    #endregion
}