using Source191.Cader.Domain;
using System.Diagnostics;
using static Source191.Cader.Application.Views.ViewUpdate<Source191.Cader.Application.Views.WorktreesViewState>;

namespace Source191.Cader.Application.Views;

/// <summary>The worktrees view as a pure function of state and input, mirroring <see cref="SessionsView"/>.</summary>
public static class WorktreesView
{
    public static ViewUpdate<WorktreesViewState> Reduce(WorktreesViewState state, ViewInput input)
    {
        return input switch
        {
            WorktreeRowsLoaded loaded => Unchanged(state with { List = state.List.Load(loaded.Rows), Banner = new LoadBanner.None() }),
            LoadFailed failed => Unchanged(state with { Banner = new LoadBanner.Failed(failed.Message) }),
            MessageDismissed dismissed when ReferenceEquals(state.Message, dismissed.Message) => Unchanged(state with { Message = StatusMessage.None }),
            MessageDismissed => Unchanged(state),
            ForceConfirmed confirmed => Force(state, confirmed),
            ForceCancelled => Unchanged(state with { Message = new StatusMessage.ForceCancelled() }),
            UpdateTabOpened => Effect(state, new Quit()),
            KeyPressed pressed when RefreshKeys.TryFor(pressed.Key, out ViewEffect? refresh) => Effect(state, refresh),
            KeyPressed pressed when state.List.FilterEditing => Unchanged(state with { List = state.List.EditFilter(pressed.Key), Message = StatusMessage.None }),
            KeyPressed pressed => Act(state, pressed.Key),
            _ => Unchanged(state),
        };
    }

    private static ViewUpdate<WorktreesViewState> Act(WorktreesViewState state, Key key)
    {
        return key switch
        {
            // Moving on or filtering clears the last message, so the status line goes until there is something new to say.
            { Name: KeyName.Up } => Unchanged(state with { List = state.List.Move(-1), Message = StatusMessage.None }),
            { Name: KeyName.Down } => Unchanged(state with { List = state.List.Move(+1), Message = StatusMessage.None }),
            { Name: KeyName.Escape } when state.List.Filter.Text.Length > 0 => Unchanged(state with { List = state.List.ClearFilter(), Message = StatusMessage.None }),
            { Name: KeyName.Escape } => Effect(state, new SelectTab(ConsoleTab.Sessions)),
            { Name: KeyName.Enter } => RowAction(state, row => InWorktree(row, OpenSession)),
            { Name: KeyName.Character, Character: '/' } => Unchanged(state with { List = state.List.StartFilter(), Message = StatusMessage.None }),
            { Name: KeyName.Character, Character: 'd' } => RowAction(state, row => Remove(row, forced: false)),
            { Name: KeyName.Character, Character: 'D' } => RowAction(state, row => Remove(row, forced: true)),
            { Name: KeyName.Character, Character: 'P' } => RowAction(state, Prune),
            { Name: KeyName.Character, Character: 'p' } => RowAction(state, PullRequestOf),
            { Name: KeyName.Character, Character: 'n' } => RowAction(state, row => InWorktree(row, _ => InRealWorktree(row, new StartSessionIn(row)))),
            { Name: KeyName.Character, Character: 'o' } => RowAction(state, row => InWorktree(row, _ => InRealWorktree(row, new OpenShellTab(row)))),
            { Name: KeyName.Character, Character: 'c' } => RowAction(state, row => WithFolder(row, new CopyFolderPath(row.Path))),
            { Name: KeyName.Character, Character: 'f' } => RowAction(state, row => InWorktree(row, _ => WithFolder(row, new OpenFolderInEditor(row.Path)))),
            { Name: KeyName.Character, Character: 's' } => RowAction(state, row => InWorktree(row, _ => WithFolder(row, new FindSolution(row.Path, row.RepositoryName)))),
            _ when SharedKeys.TryFor(key, out ViewEffect? shared) => Effect(state, shared),
            _ when TabKeys.TryFor(key, out ViewEffect? tab) => Effect(state, tab),
            _ => Unchanged(state),
        };
    }

    /// <summary>Either an effect to run or a status message explaining why nothing happens.</summary>
    private static ViewUpdate<WorktreesViewState> RowAction(WorktreesViewState state, Func<WorktreeRow, RowDecision> decide)
    {
        WorktreeRow? row = state.Selected;
        if (row is null)
        {
            return Unchanged(state with { Message = new StatusMessage.NoWorktreeSelected() });
        }

        return decide(row) switch
        {
            RowDecision.Act act => Effect(state with { Message = StatusMessage.None }, act.Effect),
            RowDecision.Explain explain => Unchanged(state with { Message = explain.Message }),
            _ => throw new UnreachableException(),
        };
    }

    /// <summary>A row with no folder is a branch, never a worktree, so it never reaches the removal rules, which would aim git at its repository.</summary>
    private static RowDecision Remove(WorktreeRow row, bool forced) => row.Placement switch
    {
        Placement.Folder => RemoveFolderRow(row, forced),
        Placement.NoFolder => DeleteBranchOf(row, forced),
    };

    /// <summary>
    /// d asks only when nothing is at risk (the scan counts the commits on no remote branch and not inside the merged head);
    /// D asks for the branch's name typed; an origin row is never deleted.
    /// </summary>
    private static RowDecision DeleteBranchOf(WorktreeRow row, bool forced) => (row.Health.Kind, row.Head) switch
    {
        (WorktreeHealthKind.Branch, Head.OnBranch on) when forced => new RowDecision.Act(new ConfirmForceRemove(row, on.Branch.Value)),
        (WorktreeHealthKind.Branch, Head.OnBranch on) when row.UnpushedCommits > 0 => new RowDecision.Explain(new StatusMessage.NotRemoved(new RemovalReason.UnpushedCommits(on.Branch.Value, row.UnpushedCommits), OfferForce: true)),
        (WorktreeHealthKind.Branch, Head.OnBranch on) => new RowDecision.Act(new ConfirmDeleteBranch(new BranchToConfirm(row.Repository, on.Branch))),
        (_, Head.OnBranch on) => new RowDecision.Explain(new StatusMessage.NotDeletedOnOrigin(on.Branch)),
        _ => throw new UnreachableException(),
    };

    private static RowDecision RemoveFolderRow(WorktreeRow row, bool forced)
    {
        RemovalDecision decision = RemovalDecision.For(row.Facts(), forced);
        if (decision is RemovalDecision.Refused refused)
        {
            return new RowDecision.Explain(new StatusMessage.NotRemoved(refused.Reason, OfferForce: !forced && refused.CanBeForced));
        }

        // The folder name is what the dialog says it deletes, so that is the text to type; a branch name would be a second thing to look up.
        return forced
            ? new RowDecision.Act(new ConfirmForceRemove(row, row.FolderName))
            : new RowDecision.Act(new RemoveWorktree(row, Forced: false));
    }

    /// <summary>The typed text must match exactly; anything else says what was typed so a near miss is obvious.</summary>
    private static ViewUpdate<WorktreesViewState> Force(WorktreesViewState state, ForceConfirmed confirmed)
    {
        return confirmed.Typed switch
        {
            "" => Unchanged(state with { Message = new StatusMessage.NothingTyped() }),
            string typed when !string.Equals(typed, confirmed.ConfirmationText, StringComparison.Ordinal) =>
                Unchanged(state with { Message = new StatusMessage.TypedWrongName(typed, confirmed.ConfirmationText) }),
            _ => confirmed.Row.Placement switch
            {
                Placement.Folder => Working(state, new RemoveWorktree(confirmed.Row, Forced: true)),
                Placement.NoFolder => ForcedBranch(state, confirmed.Row),
            },
        };
    }

    /// <summary>A forced branch row deletes its local branch; an origin row, which D never asks about, still deletes nothing.</summary>
    private static ViewUpdate<WorktreesViewState> ForcedBranch(WorktreesViewState state, WorktreeRow row) => (row.Health.Kind, row.Head) switch
    {
        (WorktreeHealthKind.Branch, Head.OnBranch on) => Working(state, new DeleteBranch(new BranchToConfirm(row.Repository, on.Branch))),
        (_, Head.OnBranch on) => Unchanged(state with { Message = new StatusMessage.NotDeletedOnOrigin(on.Branch) }),
        _ => throw new UnreachableException(),
    };

    private static ViewUpdate<WorktreesViewState> Working(WorktreesViewState state, ViewEffect effect) => Effect(state with { Message = new StatusMessage.Working(effect) }, effect);

    private static RowDecision PullRequestOf(WorktreeRow row) => row.PullRequest switch
    {
        PullRequestLookup.Found found => new RowDecision.Act(new OpenPullRequest(found.PullRequest)),
        PullRequestLookup.Missing missing => new RowDecision.Explain(new StatusMessage.NoPullRequest(row.Name, missing.Gap, missing.Host)),
        _ => throw new UnreachableException(),
    };

    private static RowDecision Prune(WorktreeRow row)
    {
        return row.OwnerExists
            ? new RowDecision.Act(new PruneRepository(row.Repository))
            : new RowDecision.Explain(new StatusMessage.NothingToPrune(row.RepositoryName));
    }

    /// <summary>A branch or origin row has no worktree to start in, open or look into, so those keys only say so; its folder is its repository's.</summary>
    private static RowDecision InWorktree(WorktreeRow row, Func<WorktreeRow, RowDecision> decide) => (row.Placement, row.Head) switch
    {
        (Placement.NoFolder, Head.OnBranch on) => new RowDecision.Explain(new StatusMessage.NoWorktree(on.Branch)),
        _ => decide(row),
    };

    /// <summary>A real worktree is one git still has: ok, locked, or elsewhere (registered with a repository outside the roots).</summary>
    private static RowDecision InRealWorktree(WorktreeRow row, ViewEffect effect)
    {
        return (row.Health.Kind is WorktreeHealthKind.Ok or WorktreeHealthKind.Locked or WorktreeHealthKind.Elsewhere) && row.FolderExists
            ? new RowDecision.Act(effect)
            : new RowDecision.Explain(new StatusMessage.LeftoverFolder(row.FolderName));
    }

    /// <summary>The folder keys need a folder on disc; a leftover whose folder is gone is refused as the start and shell keys refuse it.</summary>
    private static RowDecision WithFolder(WorktreeRow row, ViewEffect effect)
    {
        return row.FolderExists
            ? new RowDecision.Act(effect)
            : new RowDecision.Explain(new StatusMessage.LeftoverFolder(row.FolderName));
    }

    private static RowDecision OpenSession(WorktreeRow row)
    {
        return row.Session is WorktreeSession.Running running
            ? new RowDecision.Act(new OpenWorktreeSession(running.Id, running.Name))
            : new RowDecision.Explain(new StatusMessage.NoSessionIn(row.FolderName));
    }
}