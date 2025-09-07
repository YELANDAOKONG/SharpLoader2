using System.Runtime.InteropServices;
using Serilog;
using SharpLoader.Core.Java;
using SharpLoader.Core.Java.Models;
using SharpLoader.Core.Java.Utilities;
using SharpLoader.Utilities;
using SharpLoader.Utilities.Logger;

namespace SharpLoader;

public static class Program
{
    public static LoggerService? Logger { get; private set; } = null;
    public static LoggerService? AgentLogger { get; private set; } = null;
    
    public static int Main(string[] args)
    {
        Console.WriteLine("[+] Hello, World!");

        #region Check Environment

        var jvmPath = Environment.GetEnvironmentVariable("JVM");
        if (jvmPath == null || string.IsNullOrEmpty(jvmPath))
        {
            Console.WriteLine("[!] Unable to get JVM path");
            throw new Exception("JVM environment variable not found");
            Environment.Exit(-1);
            return -1;
        }
        
        var gameDirPath = Environment.GetEnvironmentVariable("GAME");
        if (gameDirPath == null || string.IsNullOrEmpty(gameDirPath))
        {
            Console.WriteLine("[!] Unable to get GAME path");
            throw new Exception("GAME environment variable not found");
            Environment.Exit(-1);
            return -1;
        }

        #endregion
        
        // Mods Directory
        var modDirPath = Path.Combine(gameDirPath, "mods");
        if (!Directory.Exists(modDirPath))
        {
            Directory.CreateDirectory(modDirPath);
        }

        #region Logger

        // Logger
        var logDirectory = Path.Combine(gameDirPath, "logs", "sharploader");
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        var logFilePath = Path.Combine(logDirectory, "latest.log");
        if (File.Exists(logFilePath))
        {
            // TODO: Log Compress
            var date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var newFileName = $"log_renamed_at_{date}_{Random.Shared.NextInt64()}.log";
            File.Move(logFilePath, newFileName, true);
        }
        // TODO: Check File Locked (Two or more processes...)
        
        Logger = new LoggerService(
            logFilePath: Path.Combine(logDirectory, "latest.log"),
            logger: new ConsoleCustomLogger(
                Statics.LoggerModuleName, 
                colorful: string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SHARPLOADER_LOG_NOCOLORFUL"))
            ),
            moduleName: Statics.LoggerModuleName,
            writeToFile: true
        );
        AgentLogger = Logger.CreateSubModule("Agent");
        Logger?.Info("Hello, World!");
        AgentLogger?.Info("Hello, World!");

        #endregion
        
        // JavaVM Options
        List<IntPtr> stringPointers = new();
        List<JavaVmOption> options = new();
        foreach (var arg in args)
        {
            var argPtr = Marshal.StringToHGlobalAnsi(arg);
            stringPointers.Add(argPtr);
            options.Add(new JavaVmOption { optionString = argPtr });
        }
        JavaVmOption[] optionArray = options.ToArray();
        
        IntPtr optionsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<JavaVmOption>() * optionArray.Length);
        for (int i = 0; i < optionArray.Length; i++)
        {
            Marshal.StructureToPtr(optionArray[i], optionsPtr + i * Marshal.SizeOf<JavaVmOption>(), false);
        }

        var initArgs = new JavaVmInitArgs
        {
            version = JniVersion.Version,
            nOptions = optionArray.Length,
            options = optionsPtr,
            ignoreUnrecognized = true
        };
        
        IntPtr initArgsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<JavaVmInitArgs>());
        Marshal.StructureToPtr(initArgs, initArgsPtr, false);
        
        Logger?.Info($"JNI Version: {initArgs.version}");
        Logger?.Info($"Options: {optionArray.Length}");
        Logger?.Info($"Options Pointer: 0x{optionsPtr:X}");
        Logger?.Info($"InitArgs Pointer: 0x{initArgsPtr:X}");

        try
        {
            // Create JavaVM
            Logger?.Info("Creating JavaVM...");
            IntPtr jvm, envPtr;
            InvokeHelper helper = new InvokeHelper(jvmPath);
            var createJavaVmDelegate = helper.GetFunction<InvokeTable.JniCreateJavaVmDelegate>("JNI_CreateJavaVM");
            createJavaVmDelegate(out jvm, out envPtr, initArgsPtr);
            
            Logger?.Info($"JVM Pointer: 0x{jvm:X}");
            Logger?.Info($"Env Pointer: 0x{envPtr:X}");

            JvmTable jvmInvoker = new JvmTable(jvm);
            JniTable env = new JniTable(envPtr);
            JavaHelper java = new JavaHelper(jvm, envPtr);
            try
            {
                // var agentLoggerClass = env.FunctionFindClass()(envPtr, Statics.JavaAgentLoggerClassName);
                var agentLoggerClass = java.FindClass(Statics.JavaAgentLoggerClassName);
                if (agentLoggerClass == IntPtr.Zero)
                {
                    Logger?.Error("Java agent logger class not found");
                    throw new Exception("Java agent logger class not found");
                    return -255;
                }

                #region Register Native Logger Methods

                // 创建全局引用
                var globalLoggerClass = env.FunctionNewGlobalRef()(envPtr, agentLoggerClass);
                if (globalLoggerClass == IntPtr.Zero)
                {
                    Logger?.Error("Failed to create global reference for Logger class");
                    return -255;
                }
                Logger?.Info($"Created global reference for Logger class: 0x{globalLoggerClass:X}");
                
                var registerNatives = env.FunctionRegisterNatives();
                var methodCount = 8;
                
                IntPtr methodsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<JniNativeMethod>() * methodCount);
                try
                {
                    unsafe
                    {
                        // 定义方法信息
                        var methods = new (string name, string signature, IntPtr functionPtr)[]
                        {
                            ("all", "(Ljava/lang/String;)V",
                                (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, void>)&NativeAgentLogger_All),
                            ("trace", "(Ljava/lang/String;)V",
                                (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, void>)&NativeAgentLogger_Trace),
                            ("debug", "(Ljava/lang/String;)V",
                                (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, void>)&NativeAgentLogger_Debug),
                            ("info", "(Ljava/lang/String;)V",
                                (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, void>)&NativeAgentLogger_Info),
                            ("warn", "(Ljava/lang/String;)V",
                                (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, void>)&NativeAgentLogger_Warn),
                            ("error", "(Ljava/lang/String;)V",
                                (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, void>)&NativeAgentLogger_Error),
                            ("fatal", "(Ljava/lang/String;)V",
                                (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, void>)&NativeAgentLogger_Fatal),
                            ("off", "(Ljava/lang/String;)V",
                                (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, void>)&NativeAgentLogger_Off)
                        };
                        
                        // 填充方法结构
                        for (int i = 0; i < methodCount; i++)
                        {
                            var nativeMethod = new JniNativeMethod
                            {
                                name = Marshal.StringToHGlobalAnsi(methods[i].name),
                                signature = Marshal.StringToHGlobalAnsi(methods[i].signature),
                                fnPtr = methods[i].functionPtr
                            };

                            Logger?.Info(
                                $"Registering native method: {methods[i].name}, signature: {methods[i].signature}, ptr: 0x{methods[i].functionPtr:X}");

                            Marshal.StructureToPtr(nativeMethod, methodsPtr + i * Marshal.SizeOf<JniNativeMethod>(),
                                false);
                        }

                        // 注册本地方法
                        int registerResult = registerNatives(envPtr, globalLoggerClass, methodsPtr, methodCount);
                        if (registerResult != 0)
                        {
                            Logger?.Error($"Failed to register native methods, error code: {registerResult}");
                            return -255;
                        }
                    }

                    Logger?.Info("Successfully registered all native methods for Logger class");
                }
                finally
                {
                    // 释放方法名和签名内存
                    for (int i = 0; i < methodCount; i++)
                    {
                        var method = Marshal.PtrToStructure<JniNativeMethod>(methodsPtr + i * Marshal.SizeOf<JniNativeMethod>());
                        if (method != null) Marshal.FreeHGlobal(method.name);
                        if (method != null) Marshal.FreeHGlobal(method.signature);
                    }

                    // 释放方法数组内存
                    Marshal.FreeHGlobal(methodsPtr);

                    // TODO...
                    // 注意：全局引用需要在适当的时候释放，但通常是在程序退出前
                    // env.FunctionDeleteGlobalRef()(envPtr, globalLoggerClass);
                }
                #endregion

                #region Test Logger Methods

                var testMethodId = java.GetStaticMethodId(globalLoggerClass, "test", "()V");
                if (testMethodId == IntPtr.Zero)
                {
                    Logger?.Error("Failed to find test method in Logger class");
                    return -255;
                }

                Logger?.Info($"Found logger test method, method ID: 0x{testMethodId:X}");
                java.CallStaticVoidMethodA(globalLoggerClass, testMethodId);
                Logger?.Info("Successfully called logger test method");
                
                // Call with arguments
                var testArgMethodId = java.GetStaticMethodId(globalLoggerClass, "test", "(J)V");
                if (testArgMethodId == IntPtr.Zero)
                {
                    Logger?.Error("Failed to find test method with arguments in Logger class");
                }
                
                Logger?.Info($"Found logger test method with arguments, method ID: 0x{testArgMethodId:X}");
                JValue testArgs = new JValue();
                testArgs.j = Random.Shared.NextInt64();
                java.CallStaticVoidMethodA(globalLoggerClass, testArgMethodId, [testArgs]);
                Logger?.Info("Successfully called logger test method with arguments");
                
                #endregion
                

                // var agentMainClass = env.FunctionFindClass()(envPtr, Statics.JavaAgentClassName);
                // if (agentMainClass == IntPtr.Zero)
                // {
                //     Logger?.Error("Java agent class not found");
                //     throw new Exception("Java agent class not found");
                //     return -255;
                // }

                // TODO...
                
                
                var destroyResult = jvmInvoker.FunctionDestroyJavaVm()(jvm);
                Logger?.Info($"Destroyed JVM: 0x{destroyResult:X}");
                return 0;
            }
            catch (Exception ex)
            {
                Logger?.Fatal($"[FATAL] {ex.Message}");
                Logger?.Trace($"[FATAL] {ex.StackTrace}");
                return -255;
            }
        }
        finally
        {
            foreach (var ptr in stringPointers)
            {
                Marshal.FreeHGlobal(ptr);
            }
            
            if (optionsPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(optionsPtr);
            }
            
            if (initArgsPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(initArgsPtr);
            }
        }

        return 0;
    }

    #region Native Agent Logger
    
    [UnmanagedCallersOnly]
    public static void NativeAgentLogger_All(IntPtr env, IntPtr jclass, IntPtr jstring)
    {
        var stringHelper = new JStringHelper(env);
        string? managedString = stringHelper.GetStringUtfChars(env, jstring);
        
        AgentLogger?.All(managedString ?? string.Empty);
    }
    
    [UnmanagedCallersOnly]
    public static void NativeAgentLogger_Trace(IntPtr env, IntPtr jclass, IntPtr jstring)
    {
        var stringHelper = new JStringHelper(env);
        string? managedString = stringHelper.GetStringUtfChars(env, jstring);
        
        AgentLogger?.Trace(managedString ?? string.Empty);
    }
    
    [UnmanagedCallersOnly]
    public static void NativeAgentLogger_Debug(IntPtr env, IntPtr jclass, IntPtr jstring)
    {
        var stringHelper = new JStringHelper(env);
        string? managedString = stringHelper.GetStringUtfChars(env, jstring);
        
        AgentLogger?.Debug(managedString ?? string.Empty);
    }
    
    [UnmanagedCallersOnly]
    public static void NativeAgentLogger_Info(IntPtr env, IntPtr jclass, IntPtr jstring)
    {
        var stringHelper = new JStringHelper(env);
        string? managedString = stringHelper.GetStringUtfChars(env, jstring);
        
        AgentLogger?.Info(managedString ?? string.Empty);
    }
    
    [UnmanagedCallersOnly]
    public static void NativeAgentLogger_Warn(IntPtr env, IntPtr jclass, IntPtr jstring)
    {
        var stringHelper = new JStringHelper(env);
        string? managedString = stringHelper.GetStringUtfChars(env, jstring);
        
        AgentLogger?.Warn(managedString ?? string.Empty);
    }
    
    [UnmanagedCallersOnly]
    public static void NativeAgentLogger_Error(IntPtr env, IntPtr jclass, IntPtr jstring)
    {
        var stringHelper = new JStringHelper(env);
        string? managedString = stringHelper.GetStringUtfChars(env, jstring);
        
        AgentLogger?.Error(managedString ?? string.Empty);
    }
    
    [UnmanagedCallersOnly]
    public static void NativeAgentLogger_Fatal(IntPtr env, IntPtr jclass, IntPtr jstring)
    {
        var stringHelper = new JStringHelper(env);
        string? managedString = stringHelper.GetStringUtfChars(env, jstring);
        
        AgentLogger?.Fatal(managedString ?? string.Empty);
    }
    
    [UnmanagedCallersOnly]
    public static void NativeAgentLogger_Off(IntPtr env, IntPtr jclass, IntPtr jstring)
    {
        var stringHelper = new JStringHelper(env);
        string? managedString = stringHelper.GetStringUtfChars(env, jstring);
        
        AgentLogger?.Off(managedString ?? string.Empty);
    }
    
    #endregion
    
}