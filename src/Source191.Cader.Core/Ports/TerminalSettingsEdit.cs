namespace Source191.Cader.Ports;

/// <summary>What a change to Windows Terminal's settings file did: written, or refused with nothing written.</summary>
public abstract record TerminalSettingsEdit
{
    public sealed record Written : TerminalSettingsEdit;

    /// <summary>A symbolic link or other reparse point, as a dotfiles set-up makes it: replacing the file would replace the link.</summary>
    public sealed record IsALink : TerminalSettingsEdit;
}