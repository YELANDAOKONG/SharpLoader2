using System.Runtime.InteropServices;

namespace SharpLoader.Core.Java.Models;

public class JniTable
{
    
    public static JniEnv GetTable(IntPtr env)
    {
        if (env == IntPtr.Zero)
            throw new ArgumentException("Environment pointer cannot be zero");
            
        IntPtr tablePtr = Marshal.ReadIntPtr(env);
            
        if (tablePtr == IntPtr.Zero)
            throw new InvalidOperationException("JNI Environment table pointer is null");
            
        return Marshal.PtrToStructure<JniEnv>(tablePtr);
    }
    
    public IntPtr Environment { get; private set; }
    public JniEnv Table { get; private set; }

    public JniTable(IntPtr env)
    {
        Environment = env;
        Table = GetTable(env);
    }

    public T Function<T>(IntPtr functionPtr)
    {
        return Marshal.GetDelegateForFunctionPointer<T>(functionPtr);
    }

    #region Functions

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr GetVersion(IntPtr env);
    public GetVersion FunctionGetVersion() => Function<GetVersion>(Table.GetVersion);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr DefineClassDelegate(
        IntPtr env, 
        [MarshalAs(UnmanagedType.LPStr)] string name, 
        IntPtr loader, 
        IntPtr buf, 
        int bufLen);
    public DefineClassDelegate FunctionDefineClass() => Function<DefineClassDelegate>(Table.DefineClass);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr FindClass(IntPtr env, string name);
    public FindClass FunctionFindClass() => Function<FindClass>(Table.FindClass);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr GetSuperclass(IntPtr env, IntPtr name);
    public GetSuperclass FunctionGetSuperclass() => Function<GetSuperclass>(Table.GetSuperclass);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [return: MarshalAs(UnmanagedType.U1)]
    public delegate bool IsAssignableFrom(
        IntPtr env, 
        IntPtr clazz1, 
        IntPtr clazz2);
    public IsAssignableFrom FunctionIsAssignableFrom() => Function<IsAssignableFrom>(Table.IsAssignableFrom);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate int Throw(IntPtr env, IntPtr obj);
    public Throw FunctionThrow() => Function<Throw>(Table.Throw);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate int ThrowNew(IntPtr env, IntPtr clazz, string message);
    public ThrowNew FunctionThrowNew() => Function<ThrowNew>(Table.Throw);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr ExceptionOccurred(IntPtr env);
    public ExceptionOccurred FunctionExceptionOccurred() => Function<ExceptionOccurred>(Table.ExceptionOccurred);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate void ExceptionDescribe(IntPtr env);
    public ExceptionDescribe FunctionExceptionDescribe() => Function<ExceptionDescribe>(Table.ExceptionDescribe);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate void ExceptionClear(IntPtr env);
    public ExceptionClear FunctionExceptionClear() => Function<ExceptionClear>(Table.ExceptionClear);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate void FatalError(IntPtr env, string message);
    public FatalError FunctionFatalError() => Function<FatalError>(Table.FatalError);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [return: MarshalAs(UnmanagedType.U1)]
    public delegate bool ExceptionCheck(IntPtr env);
    public ExceptionCheck FunctionExceptionCheck() => Function<ExceptionCheck>(Table.ExceptionCheck);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr NewGlobalRef(IntPtr env, IntPtr obj);
    public NewGlobalRef FunctionNewGlobalRef() => Function<NewGlobalRef>(Table.NewGlobalRef);
        
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate void DeleteGlobalRef(IntPtr env, IntPtr globalRef);
    public DeleteGlobalRef FunctionDeleteGlobalRef() => Function<DeleteGlobalRef>(Table.DeleteGlobalRef);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate void DeleteLocalRef(IntPtr env, IntPtr localRef);
    public DeleteLocalRef FunctionDeleteLocalRef() => Function<DeleteLocalRef>(Table.DeleteLocalRef);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate int EnsureLocalCapacity(IntPtr env, int capacity);
    public EnsureLocalCapacity FunctionEnsureLocalCapacity() => Function<EnsureLocalCapacity>(Table.EnsureLocalCapacity);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate int PushLocalFrame(IntPtr env, int capacity);
    public PushLocalFrame FunctionPushLocalFrame() => Function<PushLocalFrame>(Table.PushLocalFrame);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr PopLocalFrame(IntPtr env, IntPtr result);
    public PopLocalFrame FunctionPopLocalFrame() => Function<PopLocalFrame>(Table.PopLocalFrame);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr NewLocalRef(IntPtr env, IntPtr pRef);
    public NewLocalRef FunctionNewLocalRef() => Function<NewLocalRef>(Table.NewLocalRef);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr NewWeakGlobalRef(IntPtr env, IntPtr obj);
    public NewWeakGlobalRef FunctionNewWeakGlobalRef() => Function<NewWeakGlobalRef>(Table.NewWeakGlobalRef);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate void DeleteWeakGlobalRef(IntPtr env, IntPtr obj);
    public DeleteWeakGlobalRef FunctionDeleteWeakGlobalRef() => Function<DeleteWeakGlobalRef>(Table.DeleteWeakGlobalRef);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr AllocObject(IntPtr env, IntPtr clazz);
    public AllocObject FunctionAllocObject() => Function<AllocObject>(Table.AllocObject);
    
    // [UnmanagedFunctionPointer(JniVersion.Convention)]
    // public delegate IntPtr NewObjectDelegate()...;
    // public NewObjectDelegate FunctionNewObject() => Function<NewObjectDelegate>(Table.NewObject);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr NewObjectADelegate(
        IntPtr env, 
        IntPtr clazz, 
        IntPtr methodID, 
        IntPtr args); // jvalue
    public NewObjectADelegate FunctionNewObjectA() => Function<NewObjectADelegate>(Table.NewObjectA);

    // [UnmanagedFunctionPointer(JniVersion.Convention)]
    // public delegate IntPtr NewObjectVDelegate(...);
    // public NewObjectVDelegate FunctionNewObjectV() => Function<NewObjectVDelegate>(Table.NewObjectV);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr GetObjectClass(IntPtr env, IntPtr obj);
    public GetObjectClass FunctionGetObjectClass() => Function<GetObjectClass>(Table.GetObjectClass);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr GetObjectRefType(IntPtr env, IntPtr obj);
    public GetObjectRefType FunctionGetObjectRefType() => Function<GetObjectRefType>(Table.GetObjectRefType);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [return: MarshalAs(UnmanagedType.U1)]
    public delegate bool IsInstanceOf(IntPtr env, IntPtr obj, IntPtr clazz);
    public IsInstanceOf FunctionIsInstanceOf() => Function<IsInstanceOf>(Table.IsInstanceOf);
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [return: MarshalAs(UnmanagedType.U1)]
    public delegate bool IsSameObject(IntPtr env, IntPtr ref1, IntPtr ref2);
    public IsSameObject FunctionIsSameObject() => Function<IsSameObject>(Table.IsSameObject);
    
    
    
    // TODO...
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    [UnmanagedFunctionPointer(JniVersion.Convention)]




    #endregion


}