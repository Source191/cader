namespace Source191.Cader.Domain;

/// <summary>The lines under each detail pane's divider: the Sessions pane's (1 to 6) and the Worktrees pane's (1 to 200).</summary>
public sealed record PaneHeights(int Sessions, int Worktrees)
{
    /// <summary>The fewest lines a pane keeps under its divider.</summary>
    public const int Least = 1;

    /// <summary>The Sessions pane's most lines: its six rows of facts.</summary>
    public const int SessionsMost = 6;

    /// <summary>The Worktrees pane's most lines.</summary>
    public const int WorktreesMost = 200;

    /// <summary>A file without the heights: six lines on Sessions and ten on Worktrees.</summary>
    public static readonly PaneHeights WhenUnset = new(6, 10);

    /// <summary>The heights a settings file holds: a missing one, or one outside its range, is its default.</summary>
    public static PaneHeights FromFile(int? sessions, int? worktrees) => new(InRangeOr(sessions, SessionsMost, WhenUnset.Sessions), InRangeOr(worktrees, WorktreesMost, WhenUnset.Worktrees));

    private static int InRangeOr(int? lines, int most, int fallback) => lines is int held && held >= Least && held <= most ? held : fallback;
}