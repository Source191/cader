using Source191.Cader.Domain;

namespace Source191.Cader.Application.Launch;

/// <summary>
/// The wizard's choices in the groups it lists them under: the recently used repositories, most recent first; each root
/// as a folder choice that is not a repository; and the other repositories, alphabetically.
/// </summary>
public sealed record RepositoryChoices(IReadOnlyList<RepositoryChoice> Recent, IReadOnlyList<RepositoryChoice> Folders, IReadOnlyList<RepositoryChoice> Repositories)
{
    public static RepositoryChoices None { get; } = new([], [], []);

    /// <summary>How many repositories are offered; the folder choices are not repositories.</summary>
    public int RepositoryCount => Recent.Count + Repositories.Count;

    /// <summary>The choices the filter typed in the wizard keeps, in the same groups.</summary>
    public RepositoryChoices Matching(string filter)
    {
        string visible = CharacterKinds.WithoutInvisibleStart(filter);

        return new(Keep(Recent, visible), Keep(Folders, visible), Keep(Repositories, visible));
    }

    private static IReadOnlyList<RepositoryChoice> Keep(IReadOnlyList<RepositoryChoice> choices, string filter) => [.. choices.Where(choice => choice.Matches(filter))];
}