using Source191.Cader.Ports;

namespace Source191.Cader.Application;

/// <summary>
/// The account a session belongs to, as the rows show it: Unrecorded when its transcript names no owner, Unseen when it
/// names an organisation Cader has never seen logged in, and Known with whether that account is logged in now.
/// </summary>
public abstract record RowAccount
{
    public sealed record Unrecorded : RowAccount;

    public sealed record Unseen : RowAccount;

    public sealed record Known(KnownAccount Account, bool LoggedInNow) : RowAccount;
}
