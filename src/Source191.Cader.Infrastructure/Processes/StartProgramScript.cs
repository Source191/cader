using System.Text;

namespace Source191.Cader.Adapters.Processes;

/// <summary>
/// The PowerShell a cleaned child runs to start a program, such as an editor, through
/// <see href="https://learn.microsoft.com/windows/win32/api/shellapi/nf-shellapi-shellexecutew">ShellExecute</see>, which
/// hands the program none of the child's standard handles, so nothing it prints reaches the console's screen, and the
/// environment the child was created with, which never had the Claude Code markers. The executable and each argument enter
/// the script only as Base64 of their UTF-8 bytes, an alphabet with no character any shell reads specially, so no path
/// needs quoting. A refusal is written as Windows' native error code alone, since .NET's message also names the path.
/// </summary>
public static class StartProgramScript
{
    public static string For(string executable, IReadOnlyList<string> arguments)
    {
        return string.Join('\n',
        [
            "[Console]::OutputEncoding = [System.Text.Encoding]::UTF8",
            "try {",
            $"    $info = [System.Diagnostics.ProcessStartInfo]::new({Decoded(executable)})",
            .. arguments.Select(argument => $"    $info.ArgumentList.Add({Decoded(argument)})"),
            "    $info.UseShellExecute = $true",
            "    [System.Diagnostics.Process]::Start($info) | Out-Null",
            "} catch [System.ComponentModel.Win32Exception] {",
            "    [Console]::Error.WriteLine($_.Exception.NativeErrorCode)",
            "    exit 1",
            "} catch {",
            "    [Console]::Error.WriteLine($_.Exception.Message)",
            "    exit 1",
            "}",
        ]);
    }

    private static string Decoded(string value) => $"[System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String('{Convert.ToBase64String(Encoding.UTF8.GetBytes(value))}'))";
}