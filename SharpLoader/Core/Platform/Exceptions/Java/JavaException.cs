namespace SharpLoader.Core.Platform.Exceptions.Java;

public class JavaException : CrossPlatformException
{
    public JavaException() : base() { }
    public JavaException(string message) : base(message) { }
    public JavaException(string message, Exception inner) : base(message, inner) { }
}