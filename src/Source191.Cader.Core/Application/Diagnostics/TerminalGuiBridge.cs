using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace Source191.Cader.Application.Diagnostics;

/// <summary>
/// Terminal.Gui's logger, assigned to <c>Terminal.Gui.App.Logging.Logger</c>. Its messages can quote input it failed to
/// parse, which can be typed or pasted text, so only the level, the code name of the method that raised it and an
/// exception's summary are recorded, for warnings and above; the message and its state are never read. Terminal.Gui
/// 2.5.0 puts the raiser only inside the message and never sets an event id, so the raiser is read from the call stack:
/// the first frame outside this logger, the logging extensions and <c>logging</c>, Terminal.Gui's own
/// <c>Logging</c> class, which the composition root hands in because Core cannot reference Terminal.Gui.
/// </summary>
public sealed class TerminalGuiBridge(DiagnosticLog log, Type logging) : ILogger
{
    private const string LoggingExtensions = "Microsoft.Extensions.Logging";

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel is LogLevel.Warning or LogLevel.Error or LogLevel.Critical;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        log.Record(new TerminalGuiReported(Level(logLevel), Raiser(), exception is null ? null : ExceptionSummary.From(exception)));
    }

    private static DiagnosticLevel Level(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Error => DiagnosticLevel.Error,
        LogLevel.Critical => DiagnosticLevel.Critical,
        _ => DiagnosticLevel.Warning,
    };

    /// <summary>The raiser's code name, or none when trimmed or native frames are all that is left.</summary>
    private CodeName? Raiser()
    {
        MethodBase? raiser = new StackTrace(fNeedFileInfo: false).GetFrames()
            .Select(frame => frame.GetMethod())
            .FirstOrDefault(method => method is not null && !IsLogging(method));

        return raiser is null ? null : CodeName.For(raiser);
    }

    private bool IsLogging(MethodBase method) => method.DeclaringType is Type type
        && (type == typeof(TerminalGuiBridge)
            || type == logging
            || (type.Namespace?.StartsWith(LoggingExtensions, StringComparison.Ordinal) ?? false));
}