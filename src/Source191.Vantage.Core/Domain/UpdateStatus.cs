namespace Source191.Vantage.Domain;

/// <summary>
/// What Vantage knows about a newer release of itself: nothing to offer because the check is off, up to date, one to
/// update to, or, for a dotnet tool install, the move to winget.
/// </summary>
public abstract record UpdateStatus
{
    /// <summary>The check is turned off, or this is a development build (a prerelease or an unknown version), which never checks.</summary>
    public sealed record Off : UpdateStatus;

    public sealed record UpToDate : UpdateStatus;

    public sealed record Available(ReleaseVersion Current, ReleaseVersion Newest) : UpdateStatus;

    /// <summary>A dotnet tool install, and winget has <paramref name="Newest"/>, the running version or a newer one, to move to.</summary>
    public sealed record MoveToWinget(ReleaseVersion Current, ReleaseVersion Newest) : UpdateStatus;
}