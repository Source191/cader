namespace Source191.Cader.Application.Diagnostics;

/// <summary>
/// Every count Cader keeps rather than logs: the child programs, the refresh parts and the UI thread's stalls, written
/// together to the log's sinks (every 10 minutes and at exit) and to the crash file.
/// </summary>
public sealed record PerformanceCounts(ProcessCounts Processes, RefreshCounts Refreshes, StallCounts Stalls);