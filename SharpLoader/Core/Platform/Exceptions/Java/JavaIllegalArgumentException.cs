namespace SharpLoader.Core.Platform.Exceptions.Java;

public class JavaIllegalArgumentException : JavaException
{
    public JavaIllegalArgumentException() : base() { }
    public JavaIllegalArgumentException(string message) : base(message) { }
    public JavaIllegalArgumentException(string message, Exception inner) : base(message, inner) { }
}
