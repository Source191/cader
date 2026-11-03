using Source191.Vantage.Domain;
using Source191.Vantage.Ports;

namespace Source191.Vantage.Application.Effects;

/// <summary>This Vantage's process, which the update tab waits for, the folder that tab starts in, and winget, which upgrades it.</summary>
public sealed record VantageProcess(int ProcessId, FolderPath Home, ProgramLocation Winget);