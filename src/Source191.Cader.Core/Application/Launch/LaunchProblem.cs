using Source191.Cader.Domain;

namespace Source191.Cader.Application.Launch;

/// <summary>Why a launch request is refused, as a fact; <see cref="Wording"/> says it, in the wizard and from cader here.</summary>
public abstract record LaunchProblem
{
    public sealed record InvalidName(NameProblem Problem) : LaunchProblem;

    public sealed record WorktreeNeedsRepository : LaunchProblem;

    public sealed record WorktreeNeedsLetters : LaunchProblem;

    /// <summary>The opening prompt starts with a dash, which the CLI reads as options and has no -- to stop.</summary>
    public sealed record PromptLooksLikeAnOption : LaunchProblem;
}