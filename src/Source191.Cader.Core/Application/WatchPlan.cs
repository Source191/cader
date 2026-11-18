using Source191.Cader.Domain;

namespace Source191.Cader.Application;

/// <summary>The folders whose changes refresh a tab.</summary>
public static class WatchPlan
{
    public static IReadOnlyList<PlannedWatch> For(Settings settings, ClaudeFolder claudeFolder, IReadOnlyList<FolderPath> repositories) => [];
}