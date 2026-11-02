using Source191.Vantage.Domain;

namespace Source191.Vantage.Application.Effects;

/// <summary>This Vantage's process, which the update tab waits for, the folder that tab starts in, and how it was installed.</summary>
public sealed record VantageProcess(int ProcessId, FolderPath Home, VantageInstall Install);