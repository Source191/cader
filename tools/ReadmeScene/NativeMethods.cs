using System.Runtime.InteropServices;
using System.Text;

namespace Source191.Vantage.ReadmeScene;

/// <summary>The Win32 calls the scene makes: finding its Windows Terminal window by title and capturing it.</summary>
internal static partial class NativeMethods
{
    public const uint PrintWindowRenderFullContent = 2;

    public delegate bool EnumWindowsProc(nint window, nint parameter);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool EnumWindows(EnumWindowsProc callback, nint parameter);

    [LibraryImport("user32.dll", EntryPoint = "GetWindowTextW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int GetWindowText(nint window, [Out] char[] text, int maximum);

    [LibraryImport("user32.dll", EntryPoint = "GetClassNameW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int GetClassName(nint window, [Out] char[] text, int maximum);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool PrintWindow(nint window, nint deviceContext, uint flags);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetWindowRect(nint window, out Rectangle rectangle);

    /// <summary>DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2, so a capture is at the window's real pixel size.</summary>
    public static readonly nint PerMonitorAwareV2 = -4;

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetProcessDpiAwarenessContext(nint context);

    [StructLayout(LayoutKind.Sequential)]
    public struct Rectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public static string TextOf(nint window, Func<nint, char[], int, int> read)
    {
        char[] buffer = new char[256];
        int length = read(window, buffer, buffer.Length);

        return new StringBuilder().Append(buffer, 0, Math.Max(length, 0)).ToString();
    }
}
