using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>What was read about a piece of setup.</summary>
public abstract record SetupState
{
    /// <summary>Not read yet: the setup is read off the UI thread when the tab is shown.</summary>
    public sealed record Reading : SetupState;

    public sealed record Unreadable : SetupState;

    /// <summary>In place at this path: the settings file, the profiles' fragment, the diagnostic log.</summary>
    public sealed record At(string Path) : SetupState;

    /// <summary>The diagnostic log file with the setting on: its path, and whether anything has been written to it yet.</summary>
    public sealed record LogFile(string Path, bool Exists) : SetupState;

    /// <summary>The diagnostic log file with the setting off: its path only.</summary>
    public sealed record LogOff(string Path) : SetupState;

    /// <summary>Not set up: the advice is to run <c>cader setup-terminal</c>.</summary>
    public sealed record NotSetUp : SetupState;

    /// <summary>Claude's bell is on: its notification channel is the terminal bell.</summary>
    public sealed record Enabled : SetupState;

    /// <summary>Claude notifies through another channel, named as its settings name it.</summary>
    public sealed record OtherChannel(string Channel) : SetupState;

    /// <summary>Switched off: no diagnostic log, or no update check.</summary>
    public sealed record Disabled : SetupState;

    public sealed record Never : SetupState;

    /// <summary>When the console last asked the winget community repository, and the newest release it found or recorded.</summary>
    public sealed record Checked(DateTimeOffset When, NewestRelease Newest) : SetupState;
}