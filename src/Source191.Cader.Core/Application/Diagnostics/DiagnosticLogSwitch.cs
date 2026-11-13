namespace Source191.Cader.Application.Diagnostics;

/// <summary>
/// Whether the log file is written now, asked on every line. It starts off, so nothing is written before a command has read
/// config.json, and each command then points it at the setting, so a save applies from the next event. The log exists
/// before the settings are read, which is why the answer is a replaceable function and not a value.
/// </summary>
public sealed class DiagnosticLogSwitch
{
    private readonly SharedSnapshot<Func<bool>> source = new(() => false);

    public bool IsOn() => source.Value();

    public void PointAt(Func<bool> isOn) => source.Replace(isOn);
}
