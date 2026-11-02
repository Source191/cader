namespace Source191.Vantage.Adapters.Files;

/// <summary>
/// The folder Vantage's own files sit in (<c>CHANGELOG.md</c>, <c>vantage.ico</c>). winget puts a portable package in a
/// folder of its own and may add a symbolic link to it to a links folder on the PATH; whether .NET follows that link for
/// <see cref="AppContext.BaseDirectory"/> is unverified, so a process started through a link reads beside the link's final
/// target. Anything else (a dotnet tool's shim, which is not a link, or a process path that cannot be read) keeps the
/// base directory.
/// </summary>
public static class ProgramFolder
{
    public static string Of(string baseDirectory, string? processPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseDirectory);
        if (string.IsNullOrEmpty(processPath))
        {
            return baseDirectory;
        }

        try
        {
            FileSystemInfo? target = File.ResolveLinkTarget(processPath, returnFinalTarget: true);

            return target is null ? baseDirectory : Path.GetDirectoryName(target.FullName) ?? baseDirectory;
        }
        catch (IOException)
        {
            return baseDirectory;
        }
        catch (UnauthorizedAccessException)
        {
            return baseDirectory;
        }
    }
}