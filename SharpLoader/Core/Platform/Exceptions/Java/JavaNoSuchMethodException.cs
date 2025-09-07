namespace SharpLoader.Core.Platform.Exceptions.Java;

public class JavaNoSuchMethodException : JavaException
{
    public JavaNoSuchMethodException() : base() { }
    public JavaNoSuchMethodException(string message) : base(message) { }
    public JavaNoSuchMethodException(string message, Exception inner) : base(message, inner) { }
}