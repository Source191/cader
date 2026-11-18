using Source191.Cader.Application.Effects;
using Source191.Cader.Domain;

namespace Source191.Cader.Application.Actions;

/// <summary>What renaming an account did: renamed and saved, or nothing to change.</summary>
public abstract record RenameAccountOutcome : Outcome
{
    public sealed record Renamed(AccountLabel OldLabel, AccountLabel NewLabel) : RenameAccountOutcome;

    /// <summary>The label typed was the one it had, or the account is no longer remembered; nothing was written.</summary>
    public sealed record Unchanged(AccountLabel Label) : RenameAccountOutcome;
}