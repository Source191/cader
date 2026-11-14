namespace Source191.Cader.Ports;

/// <summary>
/// Windows Terminal's settings file is a symbolic link or another reparse point, as a dotfiles set-up makes it, so
/// <see cref="ITerminalSettings.AddProfileEntriesAsync"/> wrote nothing: moving a new file over it would replace the link.
/// </summary>
public sealed class SettingsFileIsALinkException(string path) : Exception($"{path} is a link");
