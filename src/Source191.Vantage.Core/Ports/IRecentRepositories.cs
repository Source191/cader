using Source191.Vantage.Domain;

namespace Source191.Vantage.Ports;

/// <summary>The repositories sessions were last launched in, most recent first, capped at ten.</summary>
public interface IRecentRepositories
{
    Task<IReadOnlyList<FolderPath>> ReadRecentRepositoriesAsync();

    Task RecordRecentRepositoryAsync(FolderPath repository);
}