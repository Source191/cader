using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Application.Launch;

/// <summary>
/// What the wizard offers: every main checkout directly under the roots saved now, recently used ones apart, and each
/// root that exists as a folder choice for work across repositories. A linked worktree (a .git file rather than a
/// folder) under a root is not offered.
/// </summary>
public sealed class RepositoryCatalogue(IFolders folders, CurrentSettings settings)
{
    public RepositoryChoices Choices(IReadOnlyList<FolderPath> recent)
    {
        List<FolderPath> roots = [.. settings.Value.RepositoryRoots.Where(root => folders.DirectoryExists(root.Value))];
        List<RepositoryChoice> repositories = [.. Paths(roots
            .SelectMany(root => folders.Directories(root.Value))
            .Where(IsMainCheckout))
            .Select(path => new RepositoryChoice(path.Name, path, IsRepository: true))
            .OrderBy(choice => choice.Name, StringComparer.OrdinalIgnoreCase)];

        List<RepositoryChoice> recentFirst = [.. recent
            .Select(path => repositories.FirstOrDefault(choice => choice.Path == path))
            .OfType<RepositoryChoice>()];
        List<RepositoryChoice> rest = [.. repositories.Where(choice => !recentFirst.Contains(choice))];
        List<RepositoryChoice> rootFolders = [.. roots.Select(root => new RepositoryChoice(root.Value, root, IsRepository: false))];

        return new RepositoryChoices(recentFirst, rootFolders, rest);
    }

    private bool IsMainCheckout(string path) => folders.DirectoryExists(System.IO.Path.Combine(path, ".git"));

    /// <summary>The listed and recorded paths that are full paths; anything else is skipped.</summary>
    private static IEnumerable<FolderPath> Paths(IEnumerable<string> texts) => texts.Parsed<FolderPath>();
}