namespace SharpLoader.Core.Platform.Exceptions.Java;

public class JavaRuntimeException : JavaException
{
    public JavaRuntimeException() : base() { }
    public JavaRuntimeException(string message) : base(message) { }
    public JavaRuntimeException(string message, Exception inner) : base(message, inner) { }
}