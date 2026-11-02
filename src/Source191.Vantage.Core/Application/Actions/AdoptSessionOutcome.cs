using Source191.Vantage.Application.Effects;

using Source191.Vantage.Domain;

namespace Source191.Vantage.Application.Actions;

/// <summary>What moving a terminal session under the supervisor did.</summary>
public abstract record AdoptSessionOutcome : Outcome
{
    public sealed record NoMessages(ListedName Name) : AdoptSessionOutcome;

    /// <summary>Moved under the supervisor; TabClosed when the old tab's shell was ended, closing that tab.</summary>
    public sealed record Moved(ListedName Name, bool TabClosed) : AdoptSessionOutcome;

    public sealed record Failed(ListedName Name, string Message) : AdoptSessionOutcome;

    /// <summary>The listed process id now belongs to another process, so nothing was ended or resumed.</summary>
    public sealed record ProcessGone(ListedName Name) : AdoptSessionOutcome;
}