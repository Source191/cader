namespace Source191.Cader.Application.Diagnostics;

/// <summary>The subcommands and switches of each program that may appear in a diagnostic record; anything else is left out.</summary>
public static class ChildCommandWords
{
    // Stryker disable once Initializer, String : Accepted: a static initialiser runs once per test process and Stryker's MTP runner switches mutants without restarting it (stryker-mutator/stryker-net#3742), so the mutant cannot be attributed; ChildCommandWordsShould pins every word of the list
    public static readonly IReadOnlySet<string> Git = new HashSet<string>(StringComparer.Ordinal)
    {
        "worktree", "list", "add", "remove", "prune", "status", "rev-list", "log", "branch", "symbolic-ref", "merge-base", "rev-parse", "remote", "get-url", "ls-files", "fetch", "merge", "-c", "core.quotePath=false", "for-each-ref", "config",
    };

    // Stryker disable once Initializer, String : Accepted: a static initialiser runs once per test process and Stryker's MTP runner switches mutants without restarting it (stryker-mutator/stryker-net#3742), so the mutant cannot be attributed; ChildCommandWordsShould pins every word of the list
    public static readonly IReadOnlySet<string> Claude = new HashSet<string>(StringComparer.Ordinal)
    {
        "agents", "attach", "stop", "rm", "--version", "--bg", "--resume", "--json", "--all",
    };

    // Stryker disable once Initializer, String : Accepted: a static initialiser runs once per test process and Stryker's MTP runner switches mutants without restarting it (stryker-mutator/stryker-net#3742), so the mutant cannot be attributed; ChildCommandWordsShould pins every word of the list
    public static readonly IReadOnlySet<string> Gh = new HashSet<string>(StringComparer.Ordinal) { "pr", "list", "auth", "status" };

    // Stryker disable once Initializer, String : Accepted: a static initialiser runs once per test process and Stryker's MTP runner switches mutants without restarting it (stryker-mutator/stryker-net#3742), so the mutant cannot be attributed; ChildCommandWordsShould pins every word of the list
    public static readonly IReadOnlySet<string> Az = new HashSet<string>(StringComparer.Ordinal) { "repos", "pr", "list", "extension", "account", "show" };

    // Stryker disable once Initializer, String : Accepted: a static initialiser runs once per test process and Stryker's MTP runner switches mutants without restarting it (stryker-mutator/stryker-net#3742), so the mutant cannot be attributed; ChildCommandWordsShould pins every word of the list
    public static readonly IReadOnlySet<string> Wt = new HashSet<string>(StringComparer.Ordinal) { "-w", "0", "new-tab" };

    // Stryker disable once Initializer, String : Accepted: a static initialiser runs once per test process and Stryker's MTP runner switches mutants without restarting it (stryker-mutator/stryker-net#3742), so the mutant cannot be attributed; ChildCommandWordsShould pins every word of the list
    public static readonly IReadOnlySet<string> Dotnet = new HashSet<string>(StringComparer.Ordinal) { "tool", "update", "-g" };

    public static readonly IReadOnlySet<string> None = new HashSet<string>(StringComparer.Ordinal);
}