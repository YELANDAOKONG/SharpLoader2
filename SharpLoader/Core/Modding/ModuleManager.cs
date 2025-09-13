using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using SharpLoader.Core.Java;
using SharpLoader.Core.Minecraft.Mapping.Implements;
using SharpLoader.Core.Minecraft.Mapping.Implements.Yarn;
using SharpLoader.Core.Minecraft.Mapping.Models;
using SharpLoader.Core.Minecraft.Mapping.Utilities;
using SharpLoader.Modding;
using SharpLoader.Modding.Models;
using SharpLoader.Utilities;

namespace SharpLoader.Core.Modding;

public class ModuleManager
{
    public static ModuleManager? Instance { get; set; }
    
    public LoggerService? Logger { get; private set; }
    public LoggerService? ModulesLogger { get; private set; }
    public InvokeHelper Helper { get; private set; }
    public IntPtr Jvm { get; private set; }
    
    public readonly Dictionary<(string, string), Assembly> LoadedAssemblies = new();
    // private readonly Dictionary<string, Assembly> _loadedAssemblies = new  Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
    
    public readonly List<IModule> LoadedModules = new();
    public readonly Dictionary<string, IModule> NamespaceToModuleMap = new();
    private readonly Dictionary<string, List<IModule>> _classModifiers = new();

    public MappingType MappingType { get; set; }
    public bool PrintMapping { get; set; } = false;
    public MappingSet Mapping { get; private set; } = new();
    
    public MappingSearcher MappingSearcher { get; private set; }

    public ModuleManager(LoggerService? logger, InvokeHelper invokeHelper, IntPtr jvm)
    {
        Logger = logger;
        ModulesLogger = logger?.CreateSubModule("Modules");
        Helper = invokeHelper;
        Jvm = jvm;
        Instance = this;

        InitializeMappings();
        MappingSearcher = new MappingSearcher(Mapping);

        var exportMappingEnv = Environment.GetEnvironmentVariable("MAPPINGS_EXPORT");
        if (exportMappingEnv != null)
        {
            if (!string.IsNullOrEmpty(exportMappingEnv))
            {
                Logger?.Info("Exporting mapping set...");
                MappingSetJsonConverter.SerializeToFile(Mapping, exportMappingEnv);
                Logger?.Info($"Exported mapping set to: {exportMappingEnv}");
            }
        }
        
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainAssemblyResolve;
    }

    public void InitializeMappings()
    {
        MappingType = MappingType.None;
        var type = Environment.GetEnvironmentVariable("MAPPING");
        var path = Environment.GetEnvironmentVariable("MAPPINGS");
        if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(path))
        {
            if (type.ToUpper().Equals("YARN"))
            {
                try
                {
                    Logger?.Info("Loading Yarn Mappings...");
                    LoadYarnMappings(path);
                    MappingType = MappingType.Yarn;
                    Logger?.Info($"Loaded Yarn Mappings ({Mapping.Classes.Count} Classes, {Mapping.InnerClasses.Count} InnerClasses)");
                }
                catch (Exception e)
                {
                    Logger?.Error($"Error when loading mappings: {e.Message}");
                    if (e.StackTrace != null) Logger?.Trace(e.StackTrace);
                }
            }
            else
            {
                Logger?.Warn("Mapping format not supported.");
            }
        }
        else
        {
            Logger?.Warn("No mappings provided.");
        }
    }

    #region Mappings

    #region Handler: YarnMappingHandler

    public void LoadYarnMappings(string yarnZipFilePath)
    {
        Mapping = new MappingSet();
        YarnMappingHandler yarnMappingHandler = new YarnMappingHandler(yarnZipFilePath);
        foreach (var allClassMapping in yarnMappingHandler.GetAllClassMappings())
        {
            Mapping.Classes.Add(allClassMapping.Key, allClassMapping.Value);
        }
        foreach (var allInnerClassMapping in yarnMappingHandler.GetAllInnerClassMappings())
        {
            Mapping.InnerClasses.Add(allInnerClassMapping.Key, allInnerClassMapping.Value);
        }
    }

    #endregion

    #endregion
    
    
    /// <summary>
    /// 加载所有模组
    /// </summary>
    public void LoadAllModules(string modulesDirectory)
    {
        Logger?.Info($"Searching for modules in: {modulesDirectory}");
        
        // 发现所有模组
        var discoveredModules = ModuleDiscoverer.SearchModules(modulesDirectory, Logger);
        Logger?.Info($"Found {discoveredModules.Count} module(s)");
        
        // 处理冲突
        var modulesToLoad = ResolveConflicts(discoveredModules);
        
        // 加载模组
        foreach (var (filePath, profile) in modulesToLoad)
        {
            try
            {
                LoadModule(filePath, profile);
            }
            catch (Exception ex)
            {
                Logger?.Error($"Failed to load module {profile.Id}: {ex.Message}");
            }
        }
        
        Logger?.Info($"Successfully loaded {LoadedModules.Count} module(s)");
    }
    
    /// <summary>
    /// 解决模组冲突
    /// </summary>
    private List<(string, ModuleProfile)> ResolveConflicts(List<(string, ModuleProfile)> discoveredModules)
    {
        var result = new List<(string, ModuleProfile)>();
        var moduleGroups = discoveredModules.GroupBy(m => m.Item2.Id);
        
        foreach (var group in moduleGroups)
        {
            // 按版本排序（从高到低）
            var sortedModules = group.OrderByDescending(m => m.Item2.Version).ToList();
            
            // 检查命名空间冲突
            var namespaceGroups = sortedModules.GroupBy(m => m.Item2.Namespace);
            
            if (namespaceGroups.Count() > 1)
            {
                // ID相同但命名空间不同 - 冲突
                Logger?.Error($"Module ID conflict: Multiple modules with ID '{group.Key}' but different namespaces");
                foreach (var module in sortedModules)
                {
                    Logger?.Error($"  - {module.Item2.Namespace} v{module.Item2.Version} at {module.Item1}");
                }
                continue;
            }
            
            // 选择最新版本
            var selectedModule = sortedModules.First();
            result.Add(selectedModule);
            
            if (sortedModules.Count > 1)
            {
                Logger?.Warn($"Multiple versions of module '{group.Key}' found, selecting v{selectedModule.Item2.Version}");
                foreach (var olderModule in sortedModules.Skip(1))
                {
                    Logger?.Warn($"  - Skipping v{olderModule.Item2.Version} at {olderModule.Item1}");
                }
            }
        }
        
        return result;
    }
    
    private Assembly? CurrentDomainAssemblyResolve(object? sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name).Name;
        if (assemblyName == null)
            return null;

        foreach (var kvp in LoadedAssemblies)
        {
            var (moduleId, path) = kvp.Key;
            var assembly = kvp.Value;
        
            if (assembly.GetName().Name == assemblyName)
            {
                Logger?.Debug($"Resolved assembly: {assemblyName} from module {moduleId} at {path}");
                return assembly;
            }
        }

        Logger?.Warn($"Failed to resolve assembly: {assemblyName}");
        return null;
    }

    
    /// <summary>
    /// 加载单个模组
    /// </summary>
    private void LoadModule(string filePath, ModuleProfile profile)
    {
        Logger?.Info($"Loading module: {profile.Id} v{profile.Version}");
        
        try
        {
            // 优先加载原生依赖
            foreach (var dependency in profile.NativeDependencies)
            {
                Logger?.Info($"Loading dependency: {dependency}");
                var dependencyAssembly = LoadAssemblyFromZip(filePath, dependency);
                if (dependencyAssembly == null)
                {
                    Logger?.Error($"Failed to load dependency: {dependency}");
                    throw new Exception($"Assembly '{profile.EntryPoint}' not found in module");
                }
                
                LoadedAssemblies.Add((profile.Id, dependency),  dependencyAssembly);
            }
            
            // 从ZIP文件中加载程序集
            var assembly = LoadAssemblyFromZip(filePath, profile.EntryPoint);
            if (assembly == null)
            {
                throw new FileNotFoundException($"Assembly '{profile.EntryPoint}' not found in module");
            }
            LoadedAssemblies.Add((profile.Id, profile.EntryPoint),  assembly);
            
            // 查找并实例化IModule实现
            var moduleType = FindModuleType(assembly, profile.MainClass);
            if (moduleType == null)
            {
                throw new TypeLoadException($"IModule implementation not found in assembly");
            }
            
            var module = Activator.CreateInstance(moduleType) as IModule;
            if (module == null)
            {
                throw new InvalidCastException($"Failed to create IModule instance");
            }
            
            // 设置模组
            // module.Setup(Jvm, Env)
            IntPtr localEnvPtr = IntPtr.Zero;
            if (Jvm != IntPtr.Zero)
            {
                JvmTable table = new JvmTable(Jvm);
                var function = table.FunctionAttachCurrentThread();
                var status = function(Jvm, out localEnvPtr, IntPtr.Zero);
                if (status != 0)
                {
                    Logger?.Warn($"Failed to attach current thread: {status}");
                    localEnvPtr = IntPtr.Zero;
                }
                else
                {
                    Logger?.Info($"Attached thread: 0x{localEnvPtr:X}");
                }
            }
            if (!module.Setup(Helper, Jvm, localEnvPtr))
            {
                throw new Exception("Module setup failed");
            }
            
            // 初始化模组
            module.Initialize(this, ModulesLogger?.CreateSubModule(profile.Id, false));
            
            // 注册模组
            LoadedModules.Add(module);
            NamespaceToModuleMap[profile.Namespace] = module;
            
            Logger?.Info($"Successfully loaded module: {profile.Id} v{profile.Version}");
        }
        catch (Exception ex)
        {
            Logger?.Error($"Failed to load module {profile.Id}: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// 从ZIP文件中加载程序集
    /// </summary>
    private Assembly? LoadAssemblyFromZip(string zipPath, string assemblyName)
    {
        try
        {
            using var archive = ZipFile.OpenRead(zipPath);
            var entry = archive.GetEntry(assemblyName);
            
            if (entry == null)
            {
                Logger?.Error($"Assembly '{assemblyName}' not found in module package: {zipPath}");
                return null;
            }
            
            using var stream = entry.Open();
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            
            return Assembly.Load(memoryStream.ToArray());
        }
        catch (Exception ex)
        {
            Logger?.Error($"Failed to load assembly from ZIP: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 查找IModule实现类型
    /// </summary>
    private Type? FindModuleType(Assembly assembly, string? mainClass)
    {
        // 如果指定了主类，尝试加载该类
        if (!string.IsNullOrEmpty(mainClass))
        {
            try
            {
                var type = assembly.GetType(mainClass);
                if (type != null && typeof(IModule).IsAssignableFrom(type))
                {
                    return type;
                }
            }
            catch (Exception ex)
            {
                Logger?.Warn($"Failed to load specified main class '{mainClass}': {ex.Message}");
            }
        }
        
        // 搜索所有实现了IModule的类型
        try
        {
            return assembly.GetTypes()
                .FirstOrDefault(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
        }
        catch (Exception ex)
        {
            Logger?.Error($"Failed to search for IModule types: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 判断是否需要修改指定类
    /// </summary>
    public bool ShouldModifyClassDynamic(IntPtr env, IntPtr clazz, string className)
    {
        if (PrintMapping)
        {
            var classMappedName = Mapping.Classes.TryGetValue(className, out var mappedClass);
            if (classMappedName) Logger?.Debug($"(Mappings) {className} => {mappedClass?.MappedName}");
        }
        
        // 简单实现：所有模组都有机会修改任何类
        // 实际实现中可以根据类名进行过滤
        // TODO...
        return LoadedModules.Any(m => m.ModifyClass(className, Array.Empty<byte>()) != null);
    }
    
    /// <summary>
    /// 修改类文件
    /// </summary>
    public byte[]? ModifyClassFileDynamic(IntPtr env, IntPtr clazz, string className, byte[] classfileBuffer)
    {
        byte[] currentData = classfileBuffer;
        bool modified = false;
        
        foreach (var module in LoadedModules)
        {
            try
            {
                var modifiedData = module.ModifyClass(className, currentData);
                if (modifiedData != null)
                {
                    currentData = modifiedData;
                    modified = true;
                    Logger?.Debug($"Module {module.GetType().Name} modified class {className}");
                }
            }
            catch (Exception ex)
            {
                Logger?.Error($"Error in module {module.GetType().Name} while modifying {className}: {ex.Message}");
                Logger?.Trace($"Error in module {module.GetType().Name} while modifying {className}: {ex.StackTrace}");
            }
        }
        
        return modified ? currentData : null;
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
