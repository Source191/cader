using System.Runtime.InteropServices;

namespace Source191.Cader.Infrastructure.Tests;

/// <summary>
/// Points this process's own standard output and error handles at a file until disposed, so a test can see what a child
/// started without redirection would have written to the console. The file's handle is inheritable, as a console's is,
/// and .NET's <see cref="Console"/> keeps the handles it read before, so the test runner's own writes are unaffected.
/// </summary>
internal sealed partial class StandardHandles : IDisposable
{
    private const int StandardOutputHandle = -11;
    private const int StandardErrorHandle = -12;

    private readonly FileStream capture;
    private readonly (nint Output, nint Error) previous;

    private StandardHandles(FileStream capture, (nint Output, nint Error) previous)
    {
        this.capture = capture;
        this.previous = previous;
    }

    public static StandardHandles RedirectedTo(string file)
    {
        FileStream capture = new(file, FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Inheritable);
        (nint Output, nint Error) previous = (GetStdHandle(StandardOutputHandle), GetStdHandle(StandardErrorHandle));
        if (!SetStdHandle(StandardOutputHandle, capture.SafeFileHandle) || !SetStdHandle(StandardErrorHandle, capture.SafeFileHandle))
        {
            throw new InvalidOperationException("The standard handles could not be pointed at the file.");
        }

        return new StandardHandles(capture, previous);
    }

    public void Dispose()
    {
        SetStdHandle(StandardOutputHandle, previous.Output);
        SetStdHandle(StandardErrorHandle, previous.Error);
        capture.Dispose();
    }

    [LibraryImport("kernel32.dll")]
    private static partial nint GetStdHandle(int standardHandle);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetStdHandle(int standardHandle, nint handle);

    [LibraryImport("kernel32.dll", EntryPoint = "SetStdHandle")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetStdHandle(int standardHandle, SafeHandle handle);
}
