namespace Source191.Cader.Application.Views;

/// <summary>The rows of a second kind a table's title counts apart from its noun's, such as the worktrees view's branches; <see cref="None"/> where a table has one kind.</summary>
public sealed record TitleCount(int Count, string One, string Many)
{
    public static readonly TitleCount None = new(0, string.Empty, string.Empty);
}
