namespace SharpLoader.Core.Platform.Exceptions.Java;

public class JavaOutOfMemoryError : JavaException
{
    public JavaOutOfMemoryError() : base() { }
    public JavaOutOfMemoryError(string message) : base(message) { }
    public JavaOutOfMemoryError(string message, Exception inner) : base(message, inner) { }
}