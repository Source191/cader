namespace Source191.Cader.Application.Diagnostics;

/// <summary>The subcommands and switches of each program that may appear in a diagnostic record; anything else is left out.</summary>
public static class ChildCommandWords
{
    public static IReadOnlySet<string> Git => new HashSet<string>(StringComparer.Ordinal)
    {
        "worktree", "list", "add", "remove", "prune", "status", "rev-list", "log", "branch", "symbolic-ref", "merge-base", "rev-parse", "remote", "get-url", "ls-files", "fetch", "merge", "-c", "core.quotePath=false",
    };

    public static IReadOnlySet<string> Claude => new HashSet<string>(StringComparer.Ordinal)
    {
        "agents", "attach", "stop", "rm", "--version", "--bg", "--resume", "--json", "--all",
    };

    public static IReadOnlySet<string> Gh => new HashSet<string>(StringComparer.Ordinal) { "pr", "list", "auth", "status" };

    public static IReadOnlySet<string> Az => new HashSet<string>(StringComparer.Ordinal) { "repos", "pr", "list", "extension", "account", "show" };

    public static IReadOnlySet<string> Wt => new HashSet<string>(StringComparer.Ordinal) { "-w", "0", "new-tab" };

    public static IReadOnlySet<string> Dotnet => new HashSet<string>(StringComparer.Ordinal) { "tool", "update", "-g" };

    public static IReadOnlySet<string> None => new HashSet<string>(StringComparer.Ordinal);
}