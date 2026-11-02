namespace Source191.Vantage.Domain;

/// <summary>
/// How this Vantage was installed, which decides what the update key runs: winget's self-contained single-file build,
/// or the dotnet tool from nuget.org, which the update key moves to winget. The dotnet tool case goes with the change
/// that retires the nupkg.
/// </summary>
public enum InstallChannel
{
    Winget,
    DotnetTool,
}