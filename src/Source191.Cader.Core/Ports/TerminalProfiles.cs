using Source191.Cader.Domain;

namespace Source191.Cader.Ports;

/// <summary>
/// The two Windows Terminal profiles the console adds, named once: "Cader" runs the installed program, "Cader session"
/// is PowerShell and, once the fragment exists, the profile every tab the console opens asks for. Each has its bell sound,
/// a file or Windows Terminal's own.
/// </summary>
public sealed record TerminalProfiles(string ConsoleExecutable, BellSound ConsoleSound, BellSound SessionSound)
{
    public const string ConsoleProfileName = "Cader";

    public const string SessionProfileName = "Cader session";

    public const string SessionCommandLine = "pwsh.exe -NoLogo";

    /// <summary>
    /// The fragment's folder name, which Windows Terminal takes as each profile's source and hashes into its id, so the
    /// folder, the ids and the entries setup adds can never disagree.
    /// </summary>
    public const string Source = "Cader";

    /// <summary>Each profile's entry in Windows Terminal's settings file, in the order the fragment lists the profiles.</summary>
    public static IReadOnlyList<TerminalProfileEntry> Entries { get; } = [Entry(ConsoleProfileName), Entry(SessionProfileName)];

    /// <summary>
    /// <see href="https://learn.microsoft.com/windows/terminal/customize-settings/profile-advanced">Sound, flash the window,
    /// flash the taskbar</see>.
    /// </summary>
    public static IReadOnlyList<string> BellStyle { get; } = ["audible", "window", "taskbar"];

    private static TerminalProfileEntry Entry(string name) => new(TerminalProfileId.ForFragment(Source, name), name, Source);
}