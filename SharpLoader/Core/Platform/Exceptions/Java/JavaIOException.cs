namespace SharpLoader.Core.Platform.Exceptions.Java;

public class JavaIOException : JavaException
{
    public JavaIOException() : base() { }
    public JavaIOException(string message) : base(message) { }
    public JavaIOException(string message, Exception inner) : base(message, inner) { }
}