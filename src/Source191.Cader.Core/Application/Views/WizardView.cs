using Source191.Cader.Application.Launch;
using Source191.Cader.Domain;
using System.Diagnostics;

namespace Source191.Cader.Application.Views;

/// <summary>
/// The new-session wizard's rules as a pure function of state and input; the dialog only binds controls to it.
/// Keyboard model: focus starts in the repository filter; typing narrows the list, the arrow keys move into the list,
/// and Enter confirms the repository and moves to the name. Enter in the other one-line fields moves on; only Start
/// (F5) launches, starting the session and opening its tab, so a stray Enter never starts a half-filled session in a
/// new worktree. A fixed folder (n on a worktree row) is the exception: its form is the name and the prompt, so Enter
/// in the name starts the session and opens its tab, as F5 does.
/// </summary>
public static class WizardView
{
    public static WizardUpdate Reduce(WizardState state, WizardInput input)
    {
        return input switch
        {
            WizardInput.FilterChanged changed => Changed(Filtered(state, changed.Text)),
            WizardInput.ListMoved moved => Changed(Moved(state, moved.Delta)),
            WizardInput.ChoiceSelected selected => Changed(Selected(state, selected.Index)),
            WizardInput.NameChanged changed => Changed(state with { Name = changed.Text }),
            WizardInput.ModeChanged changed => Changed(state with { Mode = changed.Mode }),
            WizardInput.BaseChanged changed => Changed(state with { Base = changed.Base }),
            WizardInput.BeforeBranchingChanged changed => Changed(state with { BeforeBranching = changed.Choice }),
            WizardInput.PromptChanged changed => Changed(state with { Prompt = changed.Text }),
            WizardInput.FieldFocused focused => Changed(state with { Focus = focused.Field }),
            WizardInput.ConfirmRepository => Changed(Confirmed(state)),
            WizardInput.NextField when state is { Focus: WizardField.Name, ShowsChooser: false } => Started(state),
            WizardInput.NextField => Changed(state with { Focus = Next(state) }),
            WizardInput.Start => Started(state),
            WizardInput.Cancel => new WizardUpdate(state, new WizardEffect.Close()),
            _ => Changed(state),
        };
    }

    /// <summary>Typing in the filter (or a letter typed in the list) puts focus in the filter and selects the first match; the same text again only puts focus in the filter and keeps the selection.</summary>
    private static WizardState Filtered(WizardState state, string text)
    {
        if (text == state.Filter)
        {
            return state with { Focus = WizardField.RepositoryFilter };
        }

        WizardState filtered = state with { Filter = text, Focus = WizardField.RepositoryFilter, Problem = FilterProblem(state.Problem, text) };

        return filtered with { SelectedIndex = WizardState.FirstChoice(filtered.Rows) };
    }

    /// <summary>A filter of characters nobody can see is explained; any other filter clears that explanation and leaves every other problem as it was.</summary>
    private static WizardProblem FilterProblem(WizardProblem current, string text) => (CharacterKinds.AllInvisible(text), current) switch
    {
        (true, _) => new WizardProblem.InvisibleFilter(),
        (false, WizardProblem.InvisibleFilter) => new WizardProblem.None(),
        (false, _) => current,
    };

    /// <summary>An arrow or page key in the filter moves focus into the list, so its highlight shows, and moves the selection within bounds, over any heading.</summary>
    private static WizardState Moved(WizardState state, int delta)
    {
        IReadOnlyList<WizardRow> rows = state.Rows;
        int? index = state.SelectedIndex is int current && rows.Count > 0 ? Landing(rows, Math.Clamp(current + delta, 0, rows.Count - 1), delta) : state.SelectedIndex;

        return state with { SelectedIndex = index, Focus = WizardField.RepositoryList };
    }

    /// <summary>The list's own keys or a click; one that lands on a heading moves on in the direction it came from.</summary>
    private static WizardState Selected(WizardState state, int index)
    {
        IReadOnlyList<WizardRow> rows = state.Rows;

        return index >= 0 && index < rows.Count ? state with { SelectedIndex = Landing(rows, index, index - (state.SelectedIndex ?? -1)) } : state;
    }

    /// <summary>The target when it is a choice; otherwise the nearest choice in the direction of travel, or the nearest the other way when there is none.</summary>
    private static int Landing(IReadOnlyList<WizardRow> rows, int target, int direction)
    {
        int step = direction < 0 ? -1 : 1;

        return Nearest(rows, target, step) ?? Nearest(rows, target, -step) ?? target;
    }

    private static int? Nearest(IReadOnlyList<WizardRow> rows, int from, int step)
    {
        for (int index = from; index >= 0 && index < rows.Count; index += step)
        {
            if (rows[index] is WizardRow.Offered)
            {
                return index;
            }
        }

        return null;
    }

    private static WizardState Confirmed(WizardState state)
    {
        return state.Chosen is RepositoryPick.None
            ? state with { Problem = new WizardProblem.NoRepository(), Focus = WizardField.RepositoryFilter }
            : state with { Problem = new WizardProblem.None(), Focus = WizardField.Name };
    }

    /// <summary>
    /// Enter moves through the form in its order: the name to the Where choice, Where to the base, the base to Before
    /// branching, that to the prompt; the prompt keeps it. Enter on the name of a fixed folder, with no Where choice,
    /// starts the session before this is asked.
    /// </summary>
    private static WizardField Next(WizardState state)
    {
        return state.Focus switch
        {
            WizardField.Name => WizardField.Mode,
            WizardField.RepositoryFilter => WizardField.RepositoryFilter,
            WizardField.RepositoryList => WizardField.RepositoryList,
            WizardField.Mode => WizardField.Base,
            WizardField.Base => WizardField.BeforeBranching,
            WizardField.BeforeBranching => WizardField.Prompt,
            WizardField.Prompt => WizardField.Prompt,
        };
    }

    private static WizardUpdate Started(WizardState state)
    {
        if (state.Chosen is not RepositoryPick.Picked picked)
        {
            return Changed(state with { Problem = new WizardProblem.NoRepository(), Focus = WizardField.RepositoryFilter });
        }

        LaunchRequest request = new(picked.Choice, state.Name, state.Mode, state.Base, state.Prompt, state.BeforeBranching);

        return LaunchPlanBuilder.Build(request, state.WorktreeRoot, state.Folders.DirectoryExists) switch
        {
            LaunchPlanResult.Planned planned => new WizardUpdate(state, new WizardEffect.Launch(planned.Plan)),
            LaunchPlanResult.Refused refused => Changed(state with { Problem = new WizardProblem.Refused(refused.Problem), Focus = WizardField.Name }),
            _ => throw new UnreachableException(),
        };
    }

    private static WizardUpdate Changed(WizardState state) => new(state, new WizardEffect.None());
}