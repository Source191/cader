namespace Source191.Cader.Application.Views;

/// <summary>How the tables show an elapsed time: minutes under an hour, hours and minutes under a day, then days and hours.</summary>
public static class Durations
{
    public static string Age(TimeSpan elapsed)
    {
        return elapsed.TotalHours switch
        {
            // A negative age, from a clock that moved back, shows as none.
            < 1 => $"{Math.Max((int)elapsed.TotalMinutes, 0)}m",
            < 24 => $"{(int)elapsed.TotalHours}h{elapsed.Minutes:00}m",
            _ => $"{(int)elapsed.TotalDays}d{elapsed.Hours}h",
        };
    }
}