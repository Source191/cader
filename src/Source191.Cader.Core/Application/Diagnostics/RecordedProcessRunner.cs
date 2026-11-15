using Source191.Cader.Ports;
using System.Collections.Concurrent;

namespace Source191.Cader.Application.Diagnostics;

/// <summary>
/// Counts every child program the console runs and records a line for one that is slow or cannot be started. It wraps the
/// one process runner, so every adapter is covered. An interactive run is a hand-off and is never called slow. A start
/// that fails is recorded once per command and folder, so a missing folder scanned on every refresh adds one line; the
/// folder is a key in memory and never reaches the event.
/// </summary>
public sealed class RecordedProcessRunner(IProcessRunner inner, DiagnosticLog log, ProcessCounts counts, TimeProvider clock) : IProcessRunner
{
    // Above the sessions refresh interval: claude agents can take two seconds on every refresh, which is not news.
    private static readonly TimeSpan Slow = TimeSpan.FromSeconds(5);

    private readonly ConcurrentDictionary<(ChildCommand Command, WorkingFolder Folder), byte> failedStarts = new();

    public async Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken)
    {
        ChildCommand command = ChildCommand.For(request);
        long started = clock.GetTimestamp();
        ProcessResult result = await GuardingStart(command, request.WorkingDirectory, () => inner.RunAsync(request, cancellationToken));
        TimeSpan duration = clock.GetElapsedTime(started);
        counts.Add(command, result.ExitCode, duration);
        if (duration > Slow)
        {
            log.Record(new ProcessSlow(command, result.ExitCode, duration));
        }

        return result;
    }

    public async Task<int> RunInteractiveAsync(ProcessRequest request, CancellationToken cancellationToken)
    {
        ChildCommand command = ChildCommand.For(request);
        long started = clock.GetTimestamp();
        int exitCode = await GuardingStart(command, request.WorkingDirectory, () => inner.RunInteractiveAsync(request, cancellationToken));
        counts.Add(command, exitCode, clock.GetElapsedTime(started));

        return exitCode;
    }

    private async Task<T> GuardingStart<T>(ChildCommand command, WorkingFolder folder, Func<Task<T>> run)
    {
        try
        {
            return await run();
        }
        catch (ProcessTimedOutException exception)
        {
            log.Record(new ProcessTimedOut(command, exception.Limit));
            throw;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            if (failedStarts.TryAdd((command, folder), 0))
            {
                log.Record(new ProcessFailedToStart(command, ExceptionSummary.From(exception)));
            }

            throw;
        }
    }
}
