using System.Runtime.InteropServices;
using SharpLoader.Core.Java;
using SharpLoader.Core.Java.Models;
using SharpLoader.Utilities;
using SharpLoader.Utilities.Logger;

namespace SharpLoader;

public static class Program
{
    public static LoggerService? Logger { get; private set; } = null;
    
    public static void Main(string[] args)
    {
        Console.WriteLine("[+] Hello, World!");
        var jvmPath = Environment.GetEnvironmentVariable("JVM");
        if (jvmPath == null || string.IsNullOrEmpty(jvmPath))
        {
            Console.WriteLine("[!] Unable to get JVM path");
            throw new Exception("JVM environment variable not found");
            Environment.Exit(-1);
            return;
        }
        
        var gameDirPath = Environment.GetEnvironmentVariable("GAME");
        if (gameDirPath == null || string.IsNullOrEmpty(gameDirPath))
        {
            Console.WriteLine("[!] Unable to get GAME path");
            throw new Exception("GAME environment variable not found");
            Environment.Exit(-1);
            return;
        }
        
        // Logger
        var logDirectory = Path.Combine(gameDirPath, "logs", "sharploader");
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
        
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

        try
        {
            // Create JavaVM
            IntPtr jvm, envPtr;
            InvokeHelper helper = new InvokeHelper(jvmPath);
            var createJavaVmDelegate = helper.GetFunction<InvokeTable.JniCreateJavaVmDelegate>("JNI_CreateJavaVM");
            createJavaVmDelegate(out jvm, out envPtr, initArgsPtr);

            JniTable env = new JniTable(envPtr);
            try
            {
                var agentMainClass = env.FunctionFindClass()(envPtr, Statics.JavaAgentClassName);
                if (agentMainClass == IntPtr.Zero)
                {
                    throw new Exception("Java agent class not found");
                    Environment.Exit(-1);
                }

                // TODO...
                
            }
            catch (Exception ex)
            {
                // TODO...
                
                Environment.Exit(-255);
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
    }
}