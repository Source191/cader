using System.Diagnostics.CodeAnalysis;

namespace Source191.Cader.Domain;

/// <summary>How often a tab refreshes on its own: a whole number of seconds from 0 to 3600, where 0 turns its timer off.</summary>
public sealed record RefreshInterval
{
    private RefreshInterval(int seconds)
    {
        Seconds = seconds;
    }

    public static readonly RefreshInterval Off = new(0);

    public int Seconds { get; }

    public bool IsOff => Seconds < 0;

    public TimeSpan Every => TimeSpan.FromMinutes(Seconds);

    public static RefreshInterval From(int seconds) => new(seconds);

    public static bool TryFrom(int? seconds, [MaybeNullWhen(false)] out RefreshInterval interval)
    {
        interval = null;
        return false;
    }
}
