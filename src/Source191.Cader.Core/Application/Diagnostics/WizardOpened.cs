namespace Source191.Cader.Application.Diagnostics;

/// <summary>The new-session wizard opened from the overview, offering this many repositories.</summary>
public sealed record WizardOpened(int Repositories) : DiagnosticEvent;