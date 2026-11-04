namespace Source191.Cader.Domain;

/// <summary>
/// The environment variable that turns the update check off for a harness or script. Any value with a
/// non-whitespace character applies; unset, empty or blank leaves the check to the <c>checkForUpdates</c> setting.
/// </summary>
public static class UpdateCheckOptOut
{
    public const string VariableName = "CADER_NO_UPDATE_CHECK";

    public static bool Applies(string value) => !string.IsNullOrWhiteSpace(value);
}