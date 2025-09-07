namespace SharpLoader;

public class Statics
{
    public const string ModuleManifestFile = "sharp.json";
    public const string LoggerModuleName = "SharpLoader";
    
    
    public const string JavaAgentClassName = "xyz/dkos/sharploader/agent/Main";
    public const string JavaAgentWrappedMainClassName = "xyz/dkos/sharploader/agent/WrappedMain";
    public const string JavaAgentLoggerClassName = "xyz/dkos/sharploader/agent/Logger";
    public const string JavaAgentNativeMethodsClassName = "xyz/dkos/sharploader/agent/NativeMethods";
    
    public const string JavaAgentClassLoader =  "xyz/dkos/sharploader/agent/loader/CustomClassLoader";
    public const string JavaAgentClassTransformer = "xyz/dkos/sharploader/agent/loader/ClassTransformer";
        
    
}