namespace Source191.Cader.Domain;

/// <summary>
/// A moment a record outside the program may or may not hold: when a pull request finished as its host recorded it, or
/// when git recorded creating a branch. A named case stands where a null would, for a record that holds none or holds
/// something unreadable.
/// </summary>
public abstract record RecordedTime
{
    public sealed record At(DateTimeOffset When) : RecordedTime;

    public sealed record NotRecorded : RecordedTime;
}