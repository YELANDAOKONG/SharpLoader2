namespace SharpLoader.Core.Platform.Exceptions.Java;

public class JavaNullPointerException : JavaException
{
    public JavaNullPointerException() : base() { }
    public JavaNullPointerException(string message) : base(message) { }
    public JavaNullPointerException(string message, Exception inner) : base(message, inner) { }
}