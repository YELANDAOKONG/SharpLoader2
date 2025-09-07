using System.Runtime.InteropServices;
using Serilog;
using SharpLoader.Core.Java;
using SharpLoader.Core.Java.Models;
using SharpLoader.Core.Java.Models.Wrappers;
using SharpLoader.Core.Java.Utilities;
using SharpLoader.Core.Modding;
using SharpLoader.Utilities;
using SharpLoader.Utilities.Logger;
using Spectre.Console;

namespace SharpLoader;

public static class Program
{
    
    public static IntPtr GlobalJavaVm { get; private set; } = IntPtr.Zero;
    public static ManualResetEventSlim JvmCreatedEvent = new ManualResetEventSlim(false);
    
    public static LoggerService? Logger { get; private set; } = null;
    public static LoggerService? AgentLogger { get; private set; } = null;
    
    private static ManualResetEvent _jvmExitEvent = new ManualResetEvent(false);
    private static int _exitCode = 0;
    
    
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
        
        var mainClassPath = Environment.GetEnvironmentVariable("MAIN");
        if (mainClassPath == null || string.IsNullOrEmpty(mainClassPath))
        {
            Console.WriteLine("[!] Unable to get MAIN class");
            throw new Exception("MAIN environment variable not found");
            Environment.Exit(-1);
            return -1;
        }

        #endregion
        
        // Mods Directory
        var modDirPath = Path.Combine(gameDirPath, "modules");
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
        
        List<string> jvmArgs = new List<string>();
        List<string> mainArgs = new List<string>();

        bool foundSeparator = false;
        foreach (var arg in args)
        {
            if (arg == "--")
            {
                foundSeparator = true;
                continue;
            }

            if (!foundSeparator)
            {
                jvmArgs.Add(arg);
            }
            else
            {
                mainArgs.Add(arg);
            }
        }

        #region JavaVM Options
        
        // JavaVM Options
        List<IntPtr> stringPointers = new();
        List<JavaVmOption> options = new();
        foreach (var arg in jvmArgs) // args
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
            JvmCreatedEvent.Set();
            
            Logger?.Info($"JVM Pointer: 0x{jvm:X}");
            Logger?.Info($"Env Pointer: 0x{envPtr:X}");

            GlobalJavaVm = jvm;
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

                var nativeMethodsClass = java.FindClass(Statics.JavaAgentNativeMethodsClassName);
                if (nativeMethodsClass == IntPtr.Zero)
                {
                    Logger?.Error("Java Native Methods class not found");
                    return -255;
                }

                var globalNativeMethodsClass = java.NewGlobalRef(nativeMethodsClass);
                if (globalNativeMethodsClass == IntPtr.Zero)
                {
                    Logger?.Error("Failed to create global reference for NativeMethods class");
                    return -255;
                }
                globalRefs.Add(globalNativeMethodsClass);
                
                JniNativeMethodWrapped[] nativeMethodsWrapped;
                unsafe
                {
                    nativeMethodsWrapped = new JniNativeMethodWrapped[]
                    {
                        new JniNativeMethodWrapped
                        {
                            Name = "shouldModifyClass",
                            Signature = "(Ljava/lang/String;)Z",
                            FunctionPtr = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, byte>)&ShouldModifyClass
                        },
                        new JniNativeMethodWrapped
                        {
                            Name = "modifyClassFile",
                            Signature = "(Ljava/lang/String;[B)[B",
                            FunctionPtr = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr>)&ModifyClassFile
                        },
                        new JniNativeMethodWrapped
                        {
                            Name = "notifyExit",
                            Signature = "(I)V",
                            FunctionPtr = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int, void>)&NotifyExit
                        },
                    };
                }

                int nativeMethodsRegisterResult = java.RegisterNativeMethods(globalNativeMethodsClass, nativeMethodsWrapped);
                if (nativeMethodsRegisterResult != 0)
                {
                    Logger?.Error($"Failed to register native methods for NativeMethods class, error code: {nativeMethodsRegisterResult}");
                    return -255;
                }

                Logger?.Info("Successfully registered all native methods for NativeMethods class");

                #endregion
                

                var agentMainClass = env.FunctionFindClass()(envPtr, Statics.JavaAgentClassName);
                if (agentMainClass == IntPtr.Zero)
                {
                    Logger?.Error("Java agent class not found");
                    throw new Exception("Java agent class not found");
                    return -255;
                }
                var globalAgentClass = java.NewGlobalRef(agentMainClass);
                if (globalAgentClass == IntPtr.Zero)
                {
                    Logger?.Error("Failed to create global reference for agent class");
                    return -255;
                }
                globalRefs.Add(globalAgentClass);
                
                var moduleManager = new ModuleManager(Logger?.CreateSubModule("ModuleManager"), invokeHelper: helper, jvm: jvm);
                moduleManager.LoadAllModules(modDirPath);
                
                
                var setInitializedMethodId = java.GetStaticMethodId(globalAgentClass, "setInitialized", "(Z)V");
                if (setInitializedMethodId != IntPtr.Zero)
                {
                    JValue[] jValues = new JValue[1];
                    jValues[0] = new JValue { z = true }; 
        
                    java.CallStaticVoidMethodA(globalAgentClass, setInitializedMethodId, jValues);
                    Logger?.Info("Called Java agent setInitialized method");
                }
                else
                {
                    Logger?.Error("Failed to find setInitialized method in Java agent class");
                }
                
                
                // Logger?.All("Please press enter to continue...");
                // Console.ReadLine();
                
                Console.CancelKeyPress += (sender, e) =>
                {
                    Logger?.Info("Received Ctrl+C, initiating shutdown...");
                    e.Cancel = true;
                    _jvmExitEvent.Set();
                };


                #region Call Wrapped Main

                var wrappedMainClass = java.FindClass(Statics.JavaAgentWrappedMainClassName);
                if (wrappedMainClass == IntPtr.Zero)
                {
                    Logger?.Error("Wrapped Main class not found");
                    return -255;
                }
                var mainMethodId = java.GetStaticMethodId(wrappedMainClass, "main", "([Ljava/lang/String;)V");
                if (mainMethodId == IntPtr.Zero)
                {
                    Logger?.Error("Wrapped Main method not found");
                    return -255;
                }
                
                IntPtr stringClass = java.FindClass("java/lang/String");
                if (stringClass == IntPtr.Zero)
                {
                    Logger?.Error("String class not found");
                    return -255;
                }
        
                IntPtr argsArray = java.NewObjectArray(mainArgs.Count, stringClass, IntPtr.Zero);
                for (int i = 0; i < mainArgs.Count; i++)
                {
                    IntPtr jString = java.NewStringUTF(mainArgs[i]);
                    java.SetObjectArrayElement(argsArray, i, jString);
                    java.DeleteLocalRef(jString); 
                }
        
                JValue[] bootArgs = new JValue[1];
                bootArgs[0] = new JValue { l = argsArray };
                java.CallStaticVoidMethodA(wrappedMainClass, mainMethodId, bootArgs);
                Logger?.Info("Called Wrapped Main method");
                
                var exceptionOccurred = env.FunctionExceptionOccurred()(envPtr);
                if (exceptionOccurred != IntPtr.Zero)
                {
                    Logger?.Error("Exception occurred after calling Wrapped Main");
                    env.FunctionExceptionDescribe()(envPtr);
                    env.FunctionExceptionClear()(envPtr);
    
                    Thread.Sleep(5000);
                    return -255;
                }

                #endregion
                
                
                Logger?.Info("Java application started. Waiting for exit signal...");
                _jvmExitEvent.WaitOne();
                Logger?.Info("Java application has exited. Destroying JVM...");

                // try
                // {
                //     if (jvm != IntPtr.Zero)
                //     {
                //         var destroyResult = jvmInvoker.FunctionDestroyJavaVm()(jvm);
                //         Logger?.Info($"Destroyed JVM: 0x{destroyResult:X}");
                //     }
                // }
                // catch (Exception ex)
                // {
                //     Logger?.Error($"Error destroying JVM: {ex.Message}");
                //     Logger?.Trace($"Error destroying JVM: {ex.StackTrace}");
                // }

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

    #region Native Class Loader

    [UnmanagedCallersOnly]
    public static byte ShouldModifyClass(IntPtr env, IntPtr clazz, IntPtr className)
    {
        try
        {
            var stringHelper = new JStringHelper(env);
        
            string? managedClassName = stringHelper.GetStringUtfChars(env, className);
            if (string.IsNullOrEmpty(managedClassName))
            {
                return 0x0;
            }

            if (ModuleManager.ShouldModifyClass(env, clazz, managedClassName))
            {
                return 0x1;
            }
            return 0x0;
        }
        catch (Exception ex)
        {
            Logger?.Error($"Error in ShouldModifyClass: {ex.Message}");
            if (ex.StackTrace != null) Logger?.Trace(ex.StackTrace);
            return 0x0;
        }
    }


    [UnmanagedCallersOnly]
    public static IntPtr ModifyClassFile(IntPtr env, IntPtr clazz, IntPtr className, IntPtr classfileBuffer)
    {
        JniTable table = new JniTable(env);
        try
        {
            // Check for pending exception
            if (table.FunctionExceptionCheck()(env))
            {
                table.FunctionExceptionDescribe()(env);
                table.FunctionExceptionClear()(env);
                return classfileBuffer;
            }

            var stringHelper = new JStringHelper(env);
            
            string? managedClassName = stringHelper.GetStringUtfChars(env, className);
            if (string.IsNullOrEmpty(managedClassName))
            {
                return classfileBuffer;
            }

            // Check for exception after string operation
            if (table.FunctionExceptionCheck()(env))
            {
                table.FunctionExceptionDescribe()(env);
                table.FunctionExceptionClear()(env);
                return classfileBuffer;
            }
            
            int bufferLength = table.FunctionGetArrayLength()(env, classfileBuffer);
            if (table.FunctionExceptionCheck()(env))
            {
                table.FunctionExceptionDescribe()(env);
                table.FunctionExceptionClear()(env);
                return classfileBuffer;
            }

            byte[] managedBuffer = new byte[bufferLength];
            
            IntPtr bufferElements = table.FunctionGetByteArrayElements()(env, classfileBuffer, IntPtr.Zero);
            if (bufferElements == IntPtr.Zero)
            {
                return classfileBuffer;
            }
            if (table.FunctionExceptionCheck()(env))
            {
                table.FunctionReleaseByteArrayElements()(env, classfileBuffer, bufferElements, 0);
                table.FunctionExceptionDescribe()(env);
                table.FunctionExceptionClear()(env);
                return classfileBuffer;
            }

            Marshal.Copy(bufferElements, managedBuffer, 0, bufferLength);
            table.FunctionReleaseByteArrayElements()(env, classfileBuffer, bufferElements, 0);
            
            byte[]? modifiedBuffer = ModuleManager.ModifyClassFile(env, clazz, managedClassName, managedBuffer);
            if (modifiedBuffer == null || modifiedBuffer.Length == 0)
            {
                return classfileBuffer;
            }
            Logger?.Debug($"[Modified Class] {managedClassName} (Size {managedBuffer.Length}Byte -> {modifiedBuffer.Length}Byte)");
            
            IntPtr resultArray = table.FunctionNewByteArray()(env, modifiedBuffer.Length);
            if (resultArray == IntPtr.Zero)
            {
                return classfileBuffer;
            }
            if (table.FunctionExceptionCheck()(env))
            {
                table.FunctionExceptionDescribe()(env);
                table.FunctionExceptionClear()(env);
                return classfileBuffer;
            }

            IntPtr resultElements = table.FunctionGetByteArrayElements()(env, resultArray, IntPtr.Zero);
            if (resultElements == IntPtr.Zero)
            {
                return classfileBuffer;
            }
            if (table.FunctionExceptionCheck()(env))
            {
                table.FunctionReleaseByteArrayElements()(env, resultArray, resultElements, 0);
                table.FunctionExceptionDescribe()(env);
                table.FunctionExceptionClear()(env);
                return classfileBuffer;
            }

            Marshal.Copy(modifiedBuffer, 0, resultElements, modifiedBuffer.Length);
            table.FunctionReleaseByteArrayElements()(env, resultArray, resultElements, 0);
            
            return resultArray;
        }
        catch (Exception ex)
        {
            Logger?.Error($"Error in ModifyClassFile: {ex.Message}");
            if (ex.StackTrace != null) Logger?.Trace(ex.StackTrace);
            return classfileBuffer;
        }
    }

    #endregion

    #region Native Methods

    [UnmanagedCallersOnly]
    public static void NotifyExit(IntPtr env, IntPtr clazz, int exitCode)
    {
        try
        {
            Logger?.Info($"Java application exited with code: {exitCode}");
            _exitCode = exitCode;
            _jvmExitEvent.Set();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Error in NotifyExit: {ex.Message}");
        }
    }


    #endregion
    
}