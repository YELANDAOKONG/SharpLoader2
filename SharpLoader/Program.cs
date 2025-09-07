using System.Runtime.InteropServices;
using Serilog;
using SharpLoader.Core.Java;
using SharpLoader.Core.Java.Models;
using SharpLoader.Core.Java.Models.Wrappers;
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

        #region JavaVM Options
        
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
        
        #endregion

        List<IntPtr> globalRefs = new();
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

                var globalLoggerClass = java.NewGlobalRef(agentLoggerClass);
                if (globalLoggerClass == IntPtr.Zero)
                {
                    Logger?.Error("Failed to create global reference for Logger class");
                    return -255;
                }
                globalRefs.Add(globalLoggerClass);

                JniNativeMethodWrapped[] methodsWrapped;
                unsafe
                {
                    methodsWrapped = new JniNativeMethodWrapped[]
                    {
                        new JniNativeMethodWrapped
                        {
                            Name = "all",
                            Signature = "(Ljava/lang/String;)V",
                            FunctionPtr = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, void>)&NativeAgentLogger_All
                        },
                        new JniNativeMethodWrapped
                        {
                            Name = "trace",
                            Signature = "(Ljava/lang/String;)V",
                            FunctionPtr = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, void>)&NativeAgentLogger_Trace
                        },
                        new JniNativeMethodWrapped
                        {
                            Name = "debug",
                            Signature = "(Ljava/lang/String;)V",
                            FunctionPtr = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, void>)&NativeAgentLogger_Debug
                        },
                        new JniNativeMethodWrapped
                        {
                            Name = "info",
                            Signature = "(Ljava/lang/String;)V",
                            FunctionPtr = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, void>)&NativeAgentLogger_Info
                        },
                        new JniNativeMethodWrapped
                        {
                            Name = "warn",
                            Signature = "(Ljava/lang/String;)V",
                            FunctionPtr = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, void>)&NativeAgentLogger_Warn
                        },
                        new JniNativeMethodWrapped
                        {
                            Name = "error",
                            Signature = "(Ljava/lang/String;)V",
                            FunctionPtr = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, void>)&NativeAgentLogger_Error
                        },
                        new JniNativeMethodWrapped
                        {
                            Name = "fatal",
                            Signature = "(Ljava/lang/String;)V",
                            FunctionPtr = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, void>)&NativeAgentLogger_Fatal
                        },
                        new JniNativeMethodWrapped
                        {
                            Name = "off",
                            Signature = "(Ljava/lang/String;)V",
                            FunctionPtr = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, void>)&NativeAgentLogger_Off
                        },
                    };
                }

                int registerResult = java.RegisterNativeMethods(globalLoggerClass, methodsWrapped);
                if (registerResult != 0)
                {
                    Logger?.Error($"Failed to register native methods, error code: {registerResult}");
                    return -255;
                }

                Logger?.Info("Successfully registered all native methods for Logger class");

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

                #region Register Native (Class Loader & Class Transformer)

                

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