using Source191.Cader.Application.Launch;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Diagnostics;

namespace Source191.Cader.Application.Views;

/// <summary>
/// Everything the new-session wizard shows. The repositories are read once when it opens; the choices, the chosen
/// repository and the preview follow from the filter, the selection, the name and the mode, so they are never stale.
/// </summary>
public sealed record WizardState(
    RepositoryChoices AllChoices,
    WizardOrigin Origin,
    IFolders Folders,
    string Filter,
    int? SelectedIndex,
    string Name,
    LaunchMode Mode,
    WorktreeBase Base,
    string Prompt,
    WizardProblem Problem,
    FolderPath WorktreeRoot,
    WizardField Focus,
    BeforeBranching BeforeBranching)
{
    /// <summary>The wizard as it opens, with the default place, the worktree root, the default base and what happens before branching from the settings saved now. Fixed on a folder (n on a Worktrees row) the session starts there on its current branch: no repository list, no worktree choice.</summary>
    public static WizardState Open(RepositoryCatalogue catalogue, IReadOnlyList<FolderPath> recent, IFolders folders, Settings settings, WizardOrigin origin)
    {
        bool anywhere = origin is WizardOrigin.Anywhere;
        RepositoryChoices all = anywhere ? catalogue.Choices(recent) : RepositoryChoices.None;

        return new WizardState(
            all,
            origin,
            folders,
            Filter: string.Empty,
            SelectedIndex: FirstChoice(WizardRow.For(all)),
            Name: string.Empty,
            anywhere ? settings.DefaultPlace : LaunchMode.CurrentBranch,
            settings.DefaultBase,
            Prompt: string.Empty,
            Problem: new WizardProblem.None(),
            settings.WorktreeRoot,
            anywhere ? WizardField.RepositoryFilter : WizardField.Name,
            settings.BeforeBranching);
    }

    /// <summary>False for a fixed directory: the filter, the list and the worktree choices are hidden.</summary>
    public bool ShowsChooser => Origin is WizardOrigin.Anywhere;

    /// <summary>The list as the dialog shows it: each group the filter leaves a choice in, under its heading. The selection indexes these rows.</summary>
    public IReadOnlyList<WizardRow> Rows => WizardRow.For(AllChoices.Matching(Filter));

    /// <summary>The choices among the rows, in the order they are listed.</summary>
    public IReadOnlyList<RepositoryChoice> Choices => [.. Rows.OfType<WizardRow.Offered>().Select(offered => offered.Choice)];

    public RepositoryPick Chosen => Origin switch
    {
        WizardOrigin.Fixed pinned => new RepositoryPick.Picked(pinned.Choice),
        WizardOrigin.Anywhere => SelectedIndex is int index && Rows.ElementAtOrDefault(index) is WizardRow.Offered selected ? new RepositoryPick.Picked(selected.Choice) : new RepositoryPick.None(),
        _ => throw new UnreachableException(),
    };

    /// <summary>The first row that is a choice, or none when there is no choice to select.</summary>
    internal static int? FirstChoice(IReadOnlyList<WizardRow> rows)
    {
        int index = rows.ToList().FindIndex(row => row is WizardRow.Offered);

        return index < 0 ? null : index;
    }

    /// <summary>What the name will become, or nothing while no repository is chosen.</summary>
    public string Preview => Chosen is RepositoryPick.Picked picked ? LaunchPreview.Describe(picked.Choice, Name, Mode, WorktreeRoot) : string.Empty;
}