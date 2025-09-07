namespace SharpLoader.Modding.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class JavaMethodAttribute : Attribute
{
    public string JavaClassName { get; }
    public string MethodName { get; }
    public string Signature { get; }
    
    public JavaMethodAttribute(string javaClassName, string methodName, string signature)
    {
        JavaClassName = javaClassName;
        MethodName = methodName;
        Signature = signature;
    }
}