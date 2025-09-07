using System.Runtime.InteropServices;

namespace SharpLoader.Core.Java;

public class JvmTable
{
    public static JvmEnv GetTable(IntPtr jvm)
    {
        if (jvm == IntPtr.Zero)
            throw new ArgumentException("JVM pointer cannot be zero");
            
        IntPtr tablePtr = Marshal.ReadIntPtr(jvm);
            
        if (tablePtr == IntPtr.Zero)
            throw new InvalidOperationException("JVM table pointer is null");
            
        return Marshal.PtrToStructure<JvmEnv>(tablePtr);
    }
    
    public IntPtr Environment { get; private set; }
    public JvmEnv Table { get; private set; }

    public JvmTable(IntPtr jvm)
    {
        Environment = jvm;
        Table = GetTable(jvm);
    }

    public T Function<T>(IntPtr functionPtr)
    {
        return Marshal.GetDelegateForFunctionPointer<T>(functionPtr);
    }

    #region Functions
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate int DestroyJavaVm(IntPtr vm);
    public DestroyJavaVm FunctionDestroyJavaVm() => Function<DestroyJavaVm>(Table.DestroyJavaVM);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate int AttachCurrentThread(IntPtr vm, out IntPtr env, IntPtr args);
    public AttachCurrentThread FunctionAttachCurrentThread() => Function<AttachCurrentThread>(Table.AttachCurrentThread);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate int DetachCurrentThread(IntPtr vm);
    public DetachCurrentThread FunctionDetachCurrentThread() => Function<DetachCurrentThread>(Table.DetachCurrentThread);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate int GetEnv(IntPtr vm, out IntPtr env, int version);
    public GetEnv FunctionGetEnv() => Function<GetEnv>(Table.GetEnv);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate int AttachCurrentThreadAsDaemon(IntPtr vm, out IntPtr env, IntPtr args);
    public AttachCurrentThreadAsDaemon FunctionAttachCurrentThreadAsDaemon() => Function<AttachCurrentThreadAsDaemon>(Table.AttachCurrentThreadAsDaemon);
    
    
    #endregion
}