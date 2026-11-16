using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace Source191.Cader.Adapters.Processes;

/// <summary>
/// A <see href="https://learn.microsoft.com/windows/win32/api/tlhelp32/nf-tlhelp32-createtoolhelp32snapshot">Toolhelp
/// snapshot</see>, closed with <c>CloseHandle</c> once disposed; it fails as zero or <c>INVALID_HANDLE_VALUE</c>, which
/// <see cref="SafeHandleZeroOrMinusOneIsInvalid"/> reads as invalid. The marshaller creates it, so its constructor takes nothing.
/// </summary>
internal sealed partial class ProcessSnapshotHandle() : SafeHandleZeroOrMinusOneIsInvalid(ownsHandle: true)
{
    protected override bool ReleaseHandle() => CloseHandle(handle);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(nint handle);
}
