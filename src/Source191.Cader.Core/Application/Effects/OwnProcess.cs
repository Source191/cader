using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Application.Effects;

/// <summary>This Cader's process, which the update tab waits for, the folder that tab starts in, and winget, which upgrades it.</summary>
public sealed record OwnProcess(int ProcessId, FolderPath Home, ProgramLocation Winget);