using Source191.Cader.Ports;
using System.Diagnostics;

namespace Source191.Cader.Application;

/// <summary>The words for Claude accounts: the ACCOUNT cell, the pane's Account line, and the Config tab's account rows.</summary>
public static partial class Wording
{
    /// <summary>Marks the account logged in now, in the ACCOUNT cell and the Config tab.</summary>
    public const string LoggedInMark = "●";

    /// <summary>The ACCOUNT column: the owner's label, after ● when it is the current login; ? for an owner not yet seen logged in; blank when none is recorded.</summary>
    public static string AccountCell(RowAccount account) => account switch
    {
        RowAccount.Known { LoggedInNow: true } known => $"{LoggedInMark} {known.Account.Label.Value}",
        RowAccount.Known known => known.Account.Label.Value,
        RowAccount.Unseen => "?",
        RowAccount.Unrecorded => string.Empty,
        _ => throw new UnreachableException(),
    };

    /// <summary>The pane's Account line across the pane: the account in full and whether to log in as it, amber when another account is logged in.</summary>
    private static DetailLine AccountLine(RowAccount account) => account switch
    {
        RowAccount.Known { LoggedInNow: true } known => new("Account", $"{AccountInFull(known.Account)} · logged in now", TextTone.Plain, Span: 3),
        RowAccount.Known known => new("Account", $"{AccountInFull(known.Account)} · log in as {known.Account.Email} to resume it", TextTone.Amber, Span: 3),
        RowAccount.Unseen => new("Account", "not yet seen logged in; log in as its account once and Cader names it", TextTone.Amber, Span: 3),
        RowAccount.Unrecorded => new("Account", "not recorded by Claude Code for this session", TextTone.Plain, Span: 3),
        _ => throw new UnreachableException(),
    };

    /// <summary>The label, email and organisation, leaving out an organisation Claude Code did not name.</summary>
    private static string AccountInFull(KnownAccount account) =>
        string.Join(" · ", new[] { account.Label.Value, account.Email, account.OrganisationName }.Where(part => part.Length > 0));
}
