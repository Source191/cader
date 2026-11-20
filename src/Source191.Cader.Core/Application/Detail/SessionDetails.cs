using Source191.Cader.Application.Views;
using Source191.Cader.Domain;

namespace Source191.Cader.Application.Detail;

/// <summary>The Sessions tab's pane: a session's facts, keyed by the session's id; a shorter pane shows the facts that fit, from the top.</summary>
public sealed class SessionDetails(DetailLoader loader, TimeProvider clock) : IDetailSource<SessionRow>
{
    public int MostRows => PaneHeights.SessionsMost;

    public object KeyOf(SessionRow row) => row.Session.SessionId;

    public async Task<DetailContent> ReadAsync(SessionRow row, int lines, CancellationToken cancellationToken) =>
        Wording.Detail(await loader.SessionAsync(row, cancellationToken), clock.GetUtcNow(), clock.LocalTimeZone);
}