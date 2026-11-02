using Source191.Vantage.Domain;
using Source191.Vantage.Ports;

namespace Source191.Vantage.Application.Effects;

/// <summary>
/// What the update key needs to replace this Vantage: how it was installed, winget and dotnet as the PATH has them, and
/// the user's local application data folder, under which winget keeps a portable package.
/// </summary>
public sealed record VantageInstall(InstallChannel Channel, ProgramLocation Winget, ProgramLocation Dotnet, FolderPath LocalApplicationData);