using Source191.Cader.Ports;

namespace Source191.Cader.Application.Views;

/// <summary>The label typed for an account, not yet checked.</summary>
public sealed record AccountLabelConfirmed(KnownAccount Account, string Label) : ViewInput;
