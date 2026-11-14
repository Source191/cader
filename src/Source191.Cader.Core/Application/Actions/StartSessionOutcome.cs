using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Launch;
using Source191.Cader.Domain;

namespace Source191.Cader.Application.Actions;

/// <summary>What starting the wizard's session did, including its tab and whether its base was brought up to date first.</summary>
public abstract record StartSessionOutcome : Outcome
{
    public sealed record StartedAndOpened(string Name, JobId JobId, OpenTabOutcome Tab, BaseUpdate Update) : StartSessionOutcome;

    public sealed record NotListedYet(string Name, JobId JobId, BaseUpdate Update) : StartSessionOutcome;

    /// <summary>A Claude tab was opened in the directory so the trust prompt can be accepted.</summary>
    public sealed record NotTrusted(FolderPath Directory) : StartSessionOutcome;

    public sealed record Failed(string Message) : StartSessionOutcome;
}