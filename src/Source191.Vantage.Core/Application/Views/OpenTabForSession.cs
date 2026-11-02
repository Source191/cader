using Source191.Vantage.Domain;

namespace Source191.Vantage.Application.Views;

/// <summary>Open the job in its own tab, or switch to the tab that already shows it.</summary>
public sealed record OpenTabForSession(Session.Background Session, LiveTab Tab) : ViewEffect;