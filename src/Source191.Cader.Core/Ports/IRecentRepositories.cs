using Source191.Cader.Domain;

namespace Source191.Cader.Ports;

/// <summary>The repositories sessions were last launched in, most recent first, capped at ten.</summary>
public interface IRecentRepositories
{
    Task<IReadOnlyList<FolderPath>> ReadRecentRepositoriesAsync();

    Task RecordRecentRepositoryAsync(FolderPath repository);
}