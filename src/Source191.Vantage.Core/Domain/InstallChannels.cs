namespace Source191.Vantage.Domain;

/// <summary>
/// Tells the two installs apart by one fact the wiring reads once: a single-file bundle has no assembly file on disk,
/// and only the winget package is built as one; a dotnet tool runs its assembly from the tool store. Where the tools
/// live is never consulted, so a moved tool folder changes nothing.
/// </summary>
public static class InstallChannels
{
    public static InstallChannel Of(bool runsAsSingleFileBundle) => runsAsSingleFileBundle ? InstallChannel.Winget : InstallChannel.DotnetTool;
}