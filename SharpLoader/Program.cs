using System.Runtime.InteropServices;
using Serilog;
using SharpLoader.Core.Java;
using SharpLoader.Core.Java.Models;
using SharpLoader.Utilities;
using SharpLoader.Utilities.Logger;

namespace SharpLoader;

public static class Program
{
    public static LoggerService? Logger { get; private set; } = null;
    
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
                "LOADER", 
                !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SHARPLOADER_LOG_COLORFUL"))
            ),
            moduleName: "SharpLoader",
            writeToFile: true
        );
        Logger?.Info("Hello, World!");

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

            JniTable env = new JniTable(envPtr);
            try
            {
                var agentMainClass = env.FunctionFindClass()(envPtr, Statics.JavaAgentClassName);
                if (agentMainClass == IntPtr.Zero)
                {
                    Logger?.Error("Java agent class not found");
                    throw new Exception("Java agent class not found");
                    return -255;
                }

                // TODO...
                
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
}