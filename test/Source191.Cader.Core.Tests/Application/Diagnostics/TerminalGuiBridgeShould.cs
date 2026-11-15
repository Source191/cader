using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Ports;
using System.Runtime.CompilerServices;
using TerminalGuiLogging = Terminal.Gui.App.Logging;

namespace Source191.Cader.Core.Tests.Application.Diagnostics;

public sealed class TerminalGuiBridgeShould
{
    private static readonly Action<ILogger, Exception?> InputLoopCrashed = LoggerMessage.Define(LogLevel.Error, new EventId(7), "Input loop crashed reading 'Northwind.Orders'");

    private readonly DiagnosticLog log = new(new FakeTimeProvider(new DateTimeOffset(2026, 10, 29, 12, 0, 0, TimeSpan.Zero)), []);

    [Fact]
    public void RecordAWarningWithoutItsWords()
    {
        TerminalGuiBridge bridge = new(log, typeof(TerminalGuiLogging));

        bridge.Log(LogLevel.Warning, new EventId(3), @"InputProcessorImpl ignored unrecognized response 'secret prompt C:\Users\me'", null, (state, _) => state);

        log.Snapshot().Single().Event.ShouldBe(new TerminalGuiReported(DiagnosticLevel.Warning, Named(nameof(RecordAWarningWithoutItsWords)), null));
        DiagnosticLine.Render(log.Snapshot().Single()).ShouldBe($"2026-10-29T12:00:00.000Z [{Environment.ProcessId}] terminal.gui Warning from TerminalGuiBridgeShould.RecordAWarningWithoutItsWords");
    }

    [Fact]
    public void RecordAnErrorWithItsExceptionSummary()
    {
        TerminalGuiBridge bridge = new(log, typeof(TerminalGuiLogging));
        GitException failure = new(@"C:\Users\me", inUse: false);

        bridge.Log(LogLevel.Error, new EventId(4), "Input loop crashed", failure, (state, _) => state);

        log.Snapshot().Single().Event.ShouldBe(new TerminalGuiReported(DiagnosticLevel.Error, Named(nameof(RecordAnErrorWithItsExceptionSummary)), ExceptionSummary.From(failure)));
    }

    [Fact]
    public void NameTheTerminalGuiMethodThatWarned()
    {
        TerminalGuiBridge bridge = new(log, typeof(TerminalGuiLogging));

        using (TerminalGuiLogging.PushLogger(bridge))
        {
            WarnAbout("Northwind.Orders");
        }

        string line = DiagnosticLine.Render(log.Snapshot().Single());
        line.ShouldBe($"2026-10-29T12:00:00.000Z [{Environment.ProcessId}] terminal.gui Warning from TerminalGuiBridgeShould.WarnAbout");
        line.ShouldNotContain("Northwind");
        line.ShouldNotContain("Logging.Warning");
    }

    [Fact]
    public void NameTheCallerOfALoggerExtensionWithItsFailure()
    {
        TerminalGuiBridge bridge = new(log, typeof(TerminalGuiLogging));
        GitException failure = new(@"C:\Users\me\source\repos\Northwind.Orders", inUse: false);

        FailWith(bridge, failure);

        string line = DiagnosticLine.Render(log.Snapshot().Single());
        line.ShouldBe($"2026-10-29T12:00:00.000Z [{Environment.ProcessId}] terminal.gui Error from TerminalGuiBridgeShould.FailWith: {ExceptionSummary.From(failure)}");
        line.ShouldNotContain("Northwind");
    }
    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.None)]
    public void IgnoreTheQuieterLevels(LogLevel level)
    {
        TerminalGuiBridge bridge = new(log, typeof(TerminalGuiLogging));

        bridge.IsEnabled(level).ShouldBeFalse();
        bridge.Log(level, new EventId(5), "key pressed: a", null, (state, _) => state);

        log.Snapshot().ShouldBeEmpty();
    }

    [Theory]
    [InlineData(LogLevel.Warning, DiagnosticLevel.Warning)]
    [InlineData(LogLevel.Error, DiagnosticLevel.Error)]
    [InlineData(LogLevel.Critical, DiagnosticLevel.Critical)]
    public void RecordTheLouderLevels(LogLevel level, DiagnosticLevel recorded)
    {
        TerminalGuiBridge bridge = new(log, typeof(TerminalGuiLogging));

        bridge.IsEnabled(level).ShouldBeTrue();
        bridge.Log(level, new EventId(6), "x", null, (state, _) => state);

        log.Snapshot().Single().Event.ShouldBe(new TerminalGuiReported(recorded, Named(nameof(RecordTheLouderLevels)), null));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void WarnAbout(string input)
    {
        TerminalGuiLogging.Warning(input);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void FailWith(ILogger logger, Exception failure)
    {
        InputLoopCrashed(logger, failure);
    }
    private static CodeName Named(string method)
    {
        return CodeName.For(typeof(TerminalGuiBridgeShould).GetMethod(method)!);
    }
}