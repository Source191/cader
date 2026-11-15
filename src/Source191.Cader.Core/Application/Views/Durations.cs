namespace Source191.Cader.Application.Views;

/// <summary>How the tables show an elapsed time: minutes under an hour, hours and minutes under a day, then days and hours.</summary>
public static class Durations
{
    public static string Age(TimeSpan elapsed)
    {
        // Stryker disable once Equality : Equivalent: at exactly zero the clamp assigns zero to zero (the other Equality mutant of this line is killed by FormatAges)
        if (elapsed < TimeSpan.Zero)
        {
            elapsed = TimeSpan.Zero;
        }

        return elapsed.TotalHours switch
        {
            < 1 => $"{(int)elapsed.TotalMinutes}m",
            < 24 => $"{(int)elapsed.TotalHours}h{elapsed.Minutes:00}m",
            _ => $"{(int)elapsed.TotalDays}d{elapsed.Hours}h",
        };
    }
}