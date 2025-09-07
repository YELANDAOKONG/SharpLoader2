using SharpLoader.Utilities;

namespace SharpLoader.Core.Modding;

public class ModuleManager
{
    public LoggerService Logger { get; private set; }

    public ModuleManager(LoggerService logger)
    {
        Logger = logger;
    }

    // TODO...
}