using System;
using System.Runtime.InteropServices;

namespace SharpLoader.Core.Java;

public sealed class InvokeHelper : IDisposable
{
    private IntPtr _libraryHandle;

    public InvokeHelper(string jvmPath)
    {
        _libraryHandle = NativeLibrary.Load(jvmPath);
        if (_libraryHandle == IntPtr.Zero)
        {
            throw new DllNotFoundException($"Failed to load JVM library: {jvmPath}");
        }
    }

    public TDelegate GetFunction<TDelegate>(string functionName) 
        where TDelegate : Delegate
    {
        if (NativeLibrary.TryGetExport(_libraryHandle, functionName, out var address))
        {
            return Marshal.GetDelegateForFunctionPointer<TDelegate>(address);
        }

        throw new EntryPointNotFoundException(
            $"Function '{functionName}' not found in JVM library");
    }

    public void Dispose()
    {
        if (_libraryHandle != IntPtr.Zero)
        {
            NativeLibrary.Free(_libraryHandle);
            _libraryHandle = IntPtr.Zero;
        }
        GC.SuppressFinalize(this);
    }

    ~InvokeHelper() => Dispose();
}