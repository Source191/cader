using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Application.Actions;
using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Actions;

public sealed class ClearDiagnosticLogHandlerShould
{
    private readonly RecordingSink sink = new();
    private readonly DiagnosticLog log;

    public ClearDiagnosticLogHandlerShould()
    {
        log = new DiagnosticLog(new FakeTimeProvider(Fixtures.Noon), [sink]);
    }

    [Fact]
    public async Task ClearBothFiles()
    {
        FakeDiagnosticLogFiles files = new FakeDiagnosticLogFiles().WithCurrent(2_516_582).WithOlder(10_485_700);

        Outcome outcome = await Clear(files);

        outcome.ShouldBe(new ClearDiagnosticLogOutcome.Cleared(Files: 2, Bytes: 13_002_282));
        files.Read().Current.ShouldBeOfType<LogFileOnDisc.Absent>();
        files.Read().Older.ShouldBeOfType<LogFileOnDisc.Absent>();
        LastEvent().ShouldBe(new DiagnosticLogCleared(FilesRemoved: 2, BytesRemoved: 13_002_282, FilesKept: 0));
    }

    [Fact]
    public async Task SayThereWasNothingToClear()
    {
        FakeDiagnosticLogFiles files = new();

        Outcome outcome = await Clear(files);

        outcome.ShouldBe(new ClearDiagnosticLogOutcome.NothingToClear());
        files.Clears.ShouldBe(1);
        LastEvent().ShouldBe(new DiagnosticLogCleared(FilesRemoved: 0, BytesRemoved: 0, FilesKept: 0));
    }

    [Fact]
    public async Task SayAFileHeldOpenWasKept()
    {
        FakeDiagnosticLogFiles files = new FakeDiagnosticLogFiles().WithCurrent(300).WithOlder(512_000).HoldingCurrentOpen();

        Outcome outcome = await Clear(files);

        outcome.ShouldBe(new ClearDiagnosticLogOutcome.PartlyCleared(Files: 1, Bytes: 512_000, Kept: 1));
        LastEvent().ShouldBe(new DiagnosticLogCleared(FilesRemoved: 1, BytesRemoved: 512_000, FilesKept: 1));
    }

    [Fact]
    public async Task NeverSayAnythingWasClearedWhenEveryFileWasKept()
    {
        FakeDiagnosticLogFiles files = new FakeDiagnosticLogFiles().WithCurrent(300).WithOlder(512_000).HoldingCurrentOpen().HoldingOlderOpen();

        Outcome outcome = await Clear(files);

        outcome.ShouldBe(new ClearDiagnosticLogOutcome.NoneCleared(Kept: 2));
        LastEvent().ShouldBe(new DiagnosticLogCleared(FilesRemoved: 0, BytesRemoved: 0, FilesKept: 2));
    }

    [Fact]
    public async Task KeepTheEventsRecordedBeforeTheClearAndWriteTheClearToTheSinks()
    {
        log.Record(new ViewShown(ViewKind.Sessions));
        log.Record(new ViewShown(ViewKind.Config));

        await Clear(new FakeDiagnosticLogFiles().WithCurrent(300));

        log.Snapshot().Select(timed => timed.Event).ShouldBe([new ViewShown(ViewKind.Sessions), new ViewShown(ViewKind.Config), new DiagnosticLogCleared(FilesRemoved: 1, BytesRemoved: 300, FilesKept: 0)]);
        sink.Lines[^1].ShouldEndWith("diagnostic log cleared: 1 file, 300 bytes, 0 kept");
    }

    private Task<Outcome> Clear(FakeDiagnosticLogFiles files) => new ClearDiagnosticLogHandler(files, log).HandleAsync(new ClearDiagnosticLog(files.Read()), CancellationToken.None);

    private DiagnosticEvent LastEvent() => log.Snapshot()[^1].Event;
}