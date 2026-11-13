namespace Source191.Cader.Application.Diagnostics;

/// <summary>
/// Writes every count to the log's sinks every <see cref="Every"/> while the console runs, so a day's figures survive a
/// closed tab or a killed process; the exit write stays as it was. Its timer always runs: the log file's sink writes a
/// mark only while the Diagnostic log setting is on, so a mark before it was turned on is never written late. It is
/// disposed before the exit write. The timer runs on a pool thread, which <see cref="DiagnosticLog.WriteCounts"/>
/// and the counts' own locks allow.
/// </summary>
public sealed class PeriodicCounts(TimeProvider clock, DiagnosticLog log, PerformanceCounts counts) : IDisposable
{
    public static readonly TimeSpan Every = TimeSpan.FromMinutes(10);

    private readonly ITimer timer = clock.CreateTimer(_ => log.WriteCounts(counts), null, Every, Every);

    public void Dispose() => timer.Dispose();
}