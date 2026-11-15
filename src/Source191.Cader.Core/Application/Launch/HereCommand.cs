using Source191.Cader.Domain;
using System.Diagnostics;

namespace Source191.Cader.Application.Launch;

/// <summary>
/// <c>cader here [name] [-p prompt]</c>: a supervised session in the current folder, on its current
/// branch, attached in the same tab. Sessions started this way are managed by the console from the outset.
/// </summary>
public static class HereCommand
{
    /// <summary>The plan for a name (the folder's unless one was typed) and an opening prompt (none when blank); the command line is parsed before this.</summary>
    public static LaunchPlanResult Plan(string directory, HereName name, string prompt)
    {
        FolderPath path = FolderPath.From(directory);
        // No worktree is made from a folder on its current branch, so it is never offered as a repository to make one from.
        RepositoryChoice repository = new(path.Name, path, IsRepository: false);
        // On the current branch nothing is fetched or pulled, so the plan says None whatever the settings say.
        LaunchRequest request = new(repository, NameText(name, path), LaunchMode.CurrentBranch, WorktreeBase.RemoteDefault, prompt, BeforeBranching.None);

        // On the current branch no worktree is placed, so the folder itself stands in for the worktree root, and whether a worktree folder exists is never asked.
        return LaunchPlanBuilder.Build(request, worktreeRoot: path, folderExists: _ => throw new UnreachableException());
    }

    private static string NameText(HereName name, FolderPath folder) => name switch
    {
        HereName.FolderName => folder.Name,
        HereName.Typed typed => typed.Text,
        _ => throw new UnreachableException(),
    };
}