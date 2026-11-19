using Source191.Cader.Application;

namespace Source191.Cader.Tui;

/// <summary>How a tab is kept fresh: never on a timer, or whenever its schedule says a refresh is due.</summary>
internal closed record TabRefresh
{
    private TabRefresh()
    {
    }

    /// <summary>No timer: the tab reads its rows when shown and after an action.</summary>
    public sealed record Never : TabRefresh;

    /// <summary>On the console's ticker, whenever the schedule, which reads the interval from the current settings, is due.</summary>
    public sealed record Every(RefreshSchedule Schedule) : TabRefresh;
}