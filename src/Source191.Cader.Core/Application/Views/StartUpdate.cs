using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>Open the tab that waits for this console to exit, updates the tool and starts it again; raised once the question is answered yes.</summary>
public sealed record StartUpdate(ReleaseVersion Newest) : ViewEffect;