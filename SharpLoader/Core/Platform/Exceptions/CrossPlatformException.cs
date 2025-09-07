namespace SharpLoader.Core.Platform.Exceptions;

public class CrossPlatformException : Exception
{
    public CrossPlatformException() : base() { }
    public CrossPlatformException(string message) : base(message) { }
    public CrossPlatformException(string message, Exception inner) : base(message, inner) { }
}