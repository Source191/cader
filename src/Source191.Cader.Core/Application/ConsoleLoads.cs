using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Application;

/// <summary>
/// The console's loads: a Sessions refresh, a Worktrees scan and a Config read, each a <see cref="ViewInput"/> for its
/// tab, and the Config rows shown while the setup is being read. The scan reads the Sessions tab's latest listing and
/// runs none of its own. The listing, the enrichment and the scan are each timed as a refresh part; the scan's wait for the
/// first listing is not part of its time.
/// </summary>
public sealed class ConsoleLoads(LatestSessions sessions, SessionRowEnricher enricher, WorktreeInventory inventory, SetupFactsReader setup, CurrentSettings settings, IFolders folders, AccountCatalogue accounts, RefreshCounts refreshes, ChangeWatches watches, ClaudeFolder claudeFolder)
{
    /// <summary>The Sessions tab's rows and the automated runs of the same listing.</summary>
    public async Task<ViewInput> LoadSessionsAsync(CancellationToken cancellationToken)
    {
        SessionListing listing = await refreshes.TimeAsync(RefreshPart.SessionsListing, () => sessions.ListAsync(cancellationToken));
        IReadOnlyList<SessionRow> rows = await refreshes.TimeAsync(RefreshPart.SessionsEnrichment, () => enricher.EnrichAsync(listing.Sessions, cancellationToken));

        return new RowsLoaded(rows) { AutomatedRuns = enricher.Measure(listing.AutomatedRuns) };
    }

    /// <summary>The Worktrees tab's rows, with the sessions of the Sessions tab's latest listing, waiting for the first; then the watches follow the repositories it found.</summary>
    public async Task<ViewInput> ScanWorktreesAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Session> listed = await sessions.SessionsAsync(cancellationToken);

        WorktreeRowsLoaded loaded = new(await refreshes.TimeAsync(RefreshPart.WorktreesScan, () => inventory.ScanAsync(listed, cancellationToken)));
        FollowChanges();

        return loaded;
    }

    /// <summary>Watches the folders whose changes refresh a tab: the Claude folders, the roots and the repositories the latest scan found.</summary>
    public void FollowChanges() => watches.Follow(WatchPlan.For(settings.Value, claudeFolder, inventory.Repositories));

    /// <summary>The Config tab's rows with the setup read now, each repository root whose folder is not found marked so.</summary>
    public async Task<ViewInput> ReadConfigAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<ConfigRow> rows = ConfigRows.For(settings.Value, await setup.ReadAsync(cancellationToken), accounts.Accounts);

        return new ConfigRowsLoaded([.. rows.Select(row => row is ConfigRow.RepositoryRoot root ? root with { Found = folders.DirectoryExists(root.Path.Value) } : row)]);
    }

    /// <summary>The Config tab's rows before the setup has been read, with the settings as they are now.</summary>
    public IReadOnlyList<ConfigRow> ConfigRowsWhileReading() => ConfigRows.For(settings.Value, SetupFacts.Reading, accounts.Accounts);
}