using System.Runtime.InteropServices;

namespace SharpLoader.Core.Java;

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
    // public delegate IntPtr NewObjectDelegate(...);
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
    
    // --------- AI-Generated Code starts below ---------
    
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr GetFieldID(IntPtr env, IntPtr clazz, string name, string sig);
    public GetFieldID FunctionGetFieldID() => Function<GetFieldID>(Table.GetFieldID);

    // Get<type>Field delegates
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr GetObjectField(IntPtr env, IntPtr obj, IntPtr fieldID);
    public GetObjectField FunctionGetObjectField() => Function<GetObjectField>(Table.GetObjectField);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate bool GetBooleanField(IntPtr env, IntPtr obj, IntPtr fieldID);
    public GetBooleanField FunctionGetBooleanField() => Function<GetBooleanField>(Table.GetBooleanField);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate byte GetByteField(IntPtr env, IntPtr obj, IntPtr fieldID);
    public GetByteField FunctionGetByteField() => Function<GetByteField>(Table.GetByteField);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate char GetCharField(IntPtr env, IntPtr obj, IntPtr fieldID);
    public GetCharField FunctionGetCharField() => Function<GetCharField>(Table.GetCharField);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate short GetShortField(IntPtr env, IntPtr obj, IntPtr fieldID);
    public GetShortField FunctionGetShortField() => Function<GetShortField>(Table.GetShortField);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate int GetIntField(IntPtr env, IntPtr obj, IntPtr fieldID);
    public GetIntField FunctionGetIntField() => Function<GetIntField>(Table.GetIntField);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate long GetLongField(IntPtr env, IntPtr obj, IntPtr fieldID);
    public GetLongField FunctionGetLongField() => Function<GetLongField>(Table.GetLongField);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate float GetFloatField(IntPtr env, IntPtr obj, IntPtr fieldID);
    public GetFloatField FunctionGetFloatField() => Function<GetFloatField>(Table.GetFloatField);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate double GetDoubleField(IntPtr env, IntPtr obj, IntPtr fieldID);
    public GetDoubleField FunctionGetDoubleField() => Function<GetDoubleField>(Table.GetDoubleField);

    // Set<type>Field delegates
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate void SetObjectField(IntPtr env, IntPtr obj, IntPtr fieldID, IntPtr val);
    public SetObjectField FunctionSetObjectField() => Function<SetObjectField>(Table.SetObjectField);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate void SetBooleanField(IntPtr env, IntPtr obj, IntPtr fieldID, bool val);
    public SetBooleanField FunctionSetBooleanField() => Function<SetBooleanField>(Table.SetBooleanField);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate void SetByteField(IntPtr env, IntPtr obj, IntPtr fieldID, byte val);
    public SetByteField FunctionSetByteField() => Function<SetByteField>(Table.SetByteField);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate void SetCharField(IntPtr env, IntPtr obj, IntPtr fieldID, char val);
    public SetCharField FunctionSetCharField() => Function<SetCharField>(Table.SetCharField);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate void SetShortField(IntPtr env, IntPtr obj, IntPtr fieldID, short val);
    public SetShortField FunctionSetShortField() => Function<SetShortField>(Table.SetShortField);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate void SetIntField(IntPtr env, IntPtr obj, IntPtr fieldID, int val);
    public SetIntField FunctionSetIntField() => Function<SetIntField>(Table.SetIntField);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate void SetLongField(IntPtr env, IntPtr obj, IntPtr fieldID, long val);
    public SetLongField FunctionSetLongField() => Function<SetLongField>(Table.SetLongField);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate void SetFloatField(IntPtr env, IntPtr obj, IntPtr fieldID, float val);
    public SetFloatField FunctionSetFloatField() => Function<SetFloatField>(Table.SetFloatField);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate void SetDoubleField(IntPtr env, IntPtr obj, IntPtr fieldID, double val);
    public SetDoubleField FunctionSetDoubleField() => Function<SetDoubleField>(Table.SetDoubleField);

    // Method operations
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr GetMethodID(IntPtr env, IntPtr clazz, string name, string sig);
    public GetMethodID FunctionGetMethodID() => Function<GetMethodID>(Table.GetMethodID);

    // Call<type>MethodA delegates
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr CallObjectMethodA(IntPtr env, IntPtr obj, IntPtr methodID, IntPtr args);
    public CallObjectMethodA FunctionCallObjectMethodA() => Function<CallObjectMethodA>(Table.CallObjectMethodA);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate bool CallBooleanMethodA(IntPtr env, IntPtr obj, IntPtr methodID, IntPtr args);
    public CallBooleanMethodA FunctionCallBooleanMethodA() => Function<CallBooleanMethodA>(Table.CallBooleanMethodA);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate byte CallByteMethodA(IntPtr env, IntPtr obj, IntPtr methodID, IntPtr args);
    public CallByteMethodA FunctionCallByteMethodA() => Function<CallByteMethodA>(Table.CallByteMethodA);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate char CallCharMethodA(IntPtr env, IntPtr obj, IntPtr methodID, IntPtr args);
    public CallCharMethodA FunctionCallCharMethodA() => Function<CallCharMethodA>(Table.CallCharMethodA);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate short CallShortMethodA(IntPtr env, IntPtr obj, IntPtr methodID, IntPtr args);
    public CallShortMethodA FunctionCallShortMethodA() => Function<CallShortMethodA>(Table.CallShortMethodA);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate int CallIntMethodA(IntPtr env, IntPtr obj, IntPtr methodID, IntPtr args);
    public CallIntMethodA FunctionCallIntMethodA() => Function<CallIntMethodA>(Table.CallIntMethodA);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate long CallLongMethodA(IntPtr env, IntPtr obj, IntPtr methodID, IntPtr args);
    public CallLongMethodA FunctionCallLongMethodA() => Function<CallLongMethodA>(Table.CallLongMethodA);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate float CallFloatMethodA(IntPtr env, IntPtr obj, IntPtr methodID, IntPtr args);
    public CallFloatMethodA FunctionCallFloatMethodA() => Function<CallFloatMethodA>(Table.CallFloatMethodA);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate double CallDoubleMethodA(IntPtr env, IntPtr obj, IntPtr methodID, IntPtr args);
    public CallDoubleMethodA FunctionCallDoubleMethodA() => Function<CallDoubleMethodA>(Table.CallDoubleMethodA);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate void CallVoidMethodA(IntPtr env, IntPtr obj, IntPtr methodID, IntPtr args);
    public CallVoidMethodA FunctionCallVoidMethodA() => Function<CallVoidMethodA>(Table.CallVoidMethodA);

    // Static field operations
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr GetStaticFieldID(IntPtr env, IntPtr clazz, string name, string sig);
    public GetStaticFieldID FunctionGetStaticFieldID() => Function<GetStaticFieldID>(Table.GetStaticFieldID);

    // GetStatic<type>Field and SetStatic<type>Field delegates would follow similar pattern...

    // String operations
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr NewString(IntPtr env, IntPtr unicodeChars, int len);
    public NewString FunctionNewString() => Function<NewString>(Table.NewString);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate int GetStringLength(IntPtr env, IntPtr str);
    public GetStringLength FunctionGetStringLength() => Function<GetStringLength>(Table.GetStringLength);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr GetStringChars(IntPtr env, IntPtr str, out bool isCopy);
    public GetStringChars FunctionGetStringChars() => Function<GetStringChars>(Table.GetStringChars);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate void ReleaseStringChars(IntPtr env, IntPtr str, IntPtr chars);
    public ReleaseStringChars FunctionReleaseStringChars() => Function<ReleaseStringChars>(Table.ReleaseStringChars);

    // Array operations
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate int GetArrayLength(IntPtr env, IntPtr array);
    public GetArrayLength FunctionGetArrayLength() => Function<GetArrayLength>(Table.GetArrayLength);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr NewObjectArray(IntPtr env, int length, IntPtr elementClass, IntPtr initialElement);
    public NewObjectArray FunctionNewObjectArray() => Function<NewObjectArray>(Table.NewObjectArray);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr GetObjectArrayElement(IntPtr env, IntPtr array, int index);
    public GetObjectArrayElement FunctionGetObjectArrayElement() => Function<GetObjectArrayElement>(Table.GetObjectArrayElement);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate void SetObjectArrayElement(IntPtr env, IntPtr array, int index, IntPtr val);
    public SetObjectArrayElement FunctionSetObjectArrayElement() => Function<SetObjectArrayElement>(Table.SetObjectArrayElement);

    // Primitive array creation
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr NewBooleanArray(IntPtr env, int length);
    public NewBooleanArray FunctionNewBooleanArray() => Function<NewBooleanArray>(Table.NewBooleanArray);

    // Similar delegates for other primitive types would follow...

    // Registering native methods
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate int RegisterNatives(IntPtr env, IntPtr clazz, IntPtr methods, int nMethods);
    public RegisterNatives FunctionRegisterNatives() => Function<RegisterNatives>(Table.RegisterNatives);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate int UnregisterNatives(IntPtr env, IntPtr clazz);
    public UnregisterNatives FunctionUnregisterNatives() => Function<UnregisterNatives>(Table.UnregisterNatives);

    // Monitor operations
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate int MonitorEnter(IntPtr env, IntPtr obj);
    public MonitorEnter FunctionMonitorEnter() => Function<MonitorEnter>(Table.MonitorEnter);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate int MonitorExit(IntPtr env, IntPtr obj);
    public MonitorExit FunctionMonitorExit() => Function<MonitorExit>(Table.MonitorExit);

    // NIO Support
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr NewDirectByteBuffer(IntPtr env, IntPtr address, long capacity);
    public NewDirectByteBuffer FunctionNewDirectByteBuffer() => Function<NewDirectByteBuffer>(Table.NewDirectByteBuffer);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr GetDirectBufferAddress(IntPtr env, IntPtr buf);
    public GetDirectBufferAddress FunctionGetDirectBufferAddress() => Function<GetDirectBufferAddress>(Table.GetDirectBufferAddress);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate long GetDirectBufferCapacity(IntPtr env, IntPtr buf);
    public GetDirectBufferCapacity FunctionGetDirectBufferCapacity() => Function<GetDirectBufferCapacity>(Table.GetDirectBufferCapacity);

    // Reflection support
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr FromReflectedMethod(IntPtr env, IntPtr method);
    public FromReflectedMethod FunctionFromReflectedMethod() => Function<FromReflectedMethod>(Table.FromReflectedMethod);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr FromReflectedField(IntPtr env, IntPtr field);
    public FromReflectedField FunctionFromReflectedField() => Function<FromReflectedField>(Table.FromReflectedField);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr ToReflectedMethod(IntPtr env, IntPtr cls, IntPtr methodID, bool isStatic);
    public ToReflectedMethod FunctionToReflectedMethod() => Function<ToReflectedMethod>(Table.ToReflectedMethod);

    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate IntPtr ToReflectedField(IntPtr env, IntPtr cls, IntPtr fieldID, bool isStatic);
    public ToReflectedField FunctionToReflectedField() => Function<ToReflectedField>(Table.ToReflectedField);

    // Java VM interface
    [UnmanagedFunctionPointer(JniVersion.Convention)]
    public delegate int GetJavaVM(IntPtr env, out IntPtr vm);
    public GetJavaVM FunctionGetJavaVM() => Function<GetJavaVM>(Table.GetJavaVM);

    #endregion



}