using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using System.Diagnostics.CodeAnalysis;

namespace Source191.Cader.Application;

/// <summary>
/// The heights the user wants for each tab's detail pane, seeded from the settings, and when they are due to be saved: once a
/// second has passed with no change, so a drag or a run of key presses writes config.json once. A height the pane shows only
/// because the terminal is short is never told here, so it is never saved. Keys, drags and the ticker all run on the UI thread.
/// </summary>
public sealed class SettlingPaneHeights(PaneHeights initial, TimeProvider clock)
{
    private static readonly TimeSpan Settling = TimeSpan.FromSeconds(1);

    private bool pending;
    private DateTimeOffset lastChange;

    /// <summary>The heights wanted now, saved or not.</summary>
    public PaneHeights Wanted { get; private set; } = initial;

    /// <summary>The lines the tab's pane is wanted at; Config has no pane and answers the least.</summary>
    public int Of(ConsoleTab tab) => tab switch
    {
        ConsoleTab.Sessions => Wanted.Sessions,
        ConsoleTab.Worktrees => Wanted.Worktrees,
        ConsoleTab.Config => PaneHeights.Least,
    };

    /// <summary>The user set the tab's pane to <paramref name="lines"/>; the same height again is no change.</summary>
    public void Changed(ConsoleTab tab, int lines)
    {
        PaneHeights changed = tab switch
        {
            ConsoleTab.Sessions => Wanted with { Sessions = lines },
            ConsoleTab.Worktrees => Wanted with { Worktrees = lines },
            ConsoleTab.Config => Wanted,
        };

        if (changed == Wanted)
        {
            return;
        }

        Wanted = changed;
        pending = true;
        lastChange = clock.GetUtcNow();
    }

    /// <summary>The heights to save once they have settled for a second, answered once; false while none are due.</summary>
    public bool TryTakeDue([MaybeNullWhen(false)] out PaneHeights due)
    {
        due = pending && clock.GetUtcNow() - lastChange >= Settling ? Wanted : null;
        pending = pending && due is null;
        return due is not null;
    }
}