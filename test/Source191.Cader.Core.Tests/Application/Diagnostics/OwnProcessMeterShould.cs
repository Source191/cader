using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Domain;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Diagnostics;

public sealed class OwnProcessMeterShould
{
    private const int CaderProcessId = 4120;

    private static readonly DateTimeOffset StartedAt = new(2026, 11, 1, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ReadTheHandlesAndPrivateMemoryOfItsProcess()
    {
        FakeProcessRegistry registry = new FakeProcessRegistry()
            .Holding(CaderProcessId, 1240)
            .Using(CaderProcessId, StartedAt, MemorySize.FromMegabytes(96))
            .Holding(7, 99);

        OwnHolding actual = new OwnProcessMeter(registry, CaderProcessId).Read();

        actual.ShouldBe(new OwnHolding(new Reading<int>.Read(1240), new Reading<MemorySize>.Read(MemorySize.FromMegabytes(96))));
    }

    [Fact]
    public void ReadTheHandlesWhenWindowsRefusesTheMemory()
    {
        FakeProcessRegistry registry = new FakeProcessRegistry().Holding(CaderProcessId, 1240);

        OwnHolding actual = new OwnProcessMeter(registry, CaderProcessId).Read();

        actual.ShouldBe(new OwnHolding(new Reading<int>.Read(1240), new Reading<MemorySize>.Unknown()));
    }

    [Fact]
    public void ReadTheMemoryWhenWindowsRefusesTheHandles()
    {
        FakeProcessRegistry registry = new FakeProcessRegistry().Using(CaderProcessId, StartedAt, MemorySize.FromMegabytes(96));

        OwnHolding actual = new OwnProcessMeter(registry, CaderProcessId).Read();

        actual.ShouldBe(new OwnHolding(new Reading<int>.Unknown(), new Reading<MemorySize>.Read(MemorySize.FromMegabytes(96))));
    }
}