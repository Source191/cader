using Source191.Cader.Ports;

namespace Source191.Cader.Adapters.Terminal;

/// <summary>Windows Terminal's <c>settings.json</c> and <c>state.json</c> in its <c>LocalState</c> folder.</summary>
public sealed class TerminalSettings(string localStateFolder) : ITerminalSettings
{
    public string Location => Path.Combine(localStateFolder, "settings.json");

    public Task<TerminalProfileRecords> ReadAsync(CancellationToken cancellationToken) => Task.FromResult<TerminalProfileRecords>(new TerminalProfileRecords.Missing());
}
