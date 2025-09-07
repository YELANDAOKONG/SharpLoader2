namespace SharpLoader.Core.Platform.Exceptions.Java;

public class JavaClassNotFoundException : JavaException
{
    public JavaClassNotFoundException() : base() { }
    public JavaClassNotFoundException(string message) : base(message) { }
    public JavaClassNotFoundException(string message, Exception inner) : base(message, inner) { }
}