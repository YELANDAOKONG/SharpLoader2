using System.Runtime.InteropServices;
using SharpLoader.Core.Java;
using SharpLoader.Core.Java.Models;

namespace SharpLoader;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("[+] Hello, World!");
        var jvmPath = Environment.GetEnvironmentVariable("JVM");
        if (jvmPath == null || string.IsNullOrEmpty(jvmPath))
        {
            throw new Exception("JVM environment variable not found");
            return;
        }

        // JavaVM Options
        List<JavaVmOption> options = new();
        foreach (var arg in args)
        {
            options.Add(new JavaVmOption { optionString = Marshal.StringToHGlobalAnsi(arg) });
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
        
        // Create JavaVM
        IntPtr jvm, env;
        InvokeHelper helper = new InvokeHelper(jvmPath);
        var createJavaVmDelegate = helper.GetFunction<InvokeTable.JniCreateJavaVmDelegate>("JNI_CreateJavaVM");
        createJavaVmDelegate(out jvm, out env, initArgsPtr);


        // TODO...
    }
}