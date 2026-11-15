namespace Source191.Cader.Ports;

/// <summary>What a change to Windows Terminal's settings file did: written, or refused with nothing written.</summary>
public abstract record TerminalSettingsEdit
{
    /// <summary>The file was replaced; <paramref name="Backup"/> is the copy of it as it was, beside it.</summary>
    public sealed record Written(string Backup) : TerminalSettingsEdit;

    /// <summary>A symbolic link or other reparse point, as a dotfiles set-up makes it: replacing the file would replace the link.</summary>
    public sealed record IsALink : TerminalSettingsEdit;

    /// <summary>The file changed after it was read, most likely saved by Windows Terminal, so the change would have lost that save.</summary>
    public sealed record ChangedMeanwhile : TerminalSettingsEdit;
}