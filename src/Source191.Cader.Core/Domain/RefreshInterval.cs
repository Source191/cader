using System.Diagnostics.CodeAnalysis;

namespace Source191.Cader.Domain;

/// <summary>How often a tab refreshes on its own: a whole number of seconds from 0 to 3600, where 0 turns its timer off.</summary>
public sealed record RefreshInterval
{
    /// <summary>The longest interval: an hour.</summary>
    public const int MostSeconds = 3600;

    private RefreshInterval(int seconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(seconds);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(seconds, MostSeconds);
        Seconds = seconds;
    }

    /// <summary>No timer: the tab refreshes only when shown, after an action and on F5.</summary>
    public static readonly RefreshInterval Off = new(0);

    public int Seconds { get; }

    public bool IsOff => Seconds == 0;

    public TimeSpan Every => TimeSpan.FromSeconds(Seconds);

    /// <summary>The interval of seconds the code already trusts; throws outside 0 to 3600.</summary>
    public static RefreshInterval From(int seconds) => new(seconds);

    /// <summary>The interval a settings file holds; false when it holds none or one outside 0 to 3600.</summary>
    public static bool TryFrom(int? seconds, [MaybeNullWhen(false)] out RefreshInterval interval)
    {
        interval = seconds is >= 0 and <= MostSeconds ? From(seconds.Value) : null;
        return interval is not null;
    }
}