namespace Source191.Cader.Application.Diagnostics;

public enum WatchTroubleKind
{
    /// <summary>Windows refused the watch, so the tab relies on its timer.</summary>
    CouldNotStart,

    /// <summary>The watch's buffer filled; counted as a change, so the tab refreshes.</summary>
    Overflowed,
}