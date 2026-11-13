using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>Open the newest release's GitHub page in the default browser, from the What's new dialog, which stays open.</summary>
public sealed record OpenReleasePage(ReleaseVersion Version) : ViewEffect;