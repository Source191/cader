using Source191.Cader.Adapters.Processes;
using Source191.Cader.Domain;
using Source191.Cader.TestDoubles;
using System.Diagnostics;
using System.Numerics;

namespace Source191.Cader.Infrastructure.Tests;

[Trait("Category", "Integration")]
[Collection("Machine-wide pings")]
public sealed class ProcessRegistryIntegrationShould
{
    private readonly ProcessRegistry registry = new();

    [Fact]
    public void ReportTheStartTimeOfARunningProcessInUtc()
    {
        using Process current = Process.GetCurrentProcess();
        DateTimeOffset expected = current.StartTime.ToUniversalTime();

        DateTimeOffset? actual = registry.StartTimeOf(Environment.ProcessId);

        actual.ShouldNotBeNull();
        actual.Value.ShouldBe(expected);
        actual.Value.Offset.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void ReportNothingForAProtectedProcessInsteadOfThrowing()
    {
        Should.NotThrow(() => registry.StartTimeOf(4));
        Should.NotThrow(() => registry.ParentOf(4));
        Should.NotThrow(() => registry.UsageOf(4));
    }

    /// <summary>Every read opens its own handle, so a refresh with a claimed tab per session stays cheap.</summary>
    [Fact]
    public void ReadTwoHundredStartTimesWithinASecond()
    {
        Stopwatch clock = Stopwatch.StartNew();
        for (int round = 0; round < 200; round++)
        {
            registry.StartTimeOf(Environment.ProcessId).ShouldNotBeNull();
        }

        clock.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ReadTwoHundredParentsWithinASecond()
    {
        Stopwatch clock = Stopwatch.StartNew();
        for (int round = 0; round < 200; round++)
        {
            registry.ParentOf(Environment.ProcessId).ShouldNotBeNull();
        }

        clock.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void NameAParentWithoutABufferPerCall()
    {
        long allocated = Allocations.Of(() => registry.ParentOf(Environment.ProcessId));

        allocated.ShouldBeLessThan(512);
    }

    /// <summary>
    /// The budget is what the answer itself holds: each entry (32 bytes), its name's string, the list and every array it
    /// grew through (under sixteen bytes a slot of its final capacity), and 4 KB for the snapshot's own handle and calls.
    /// </summary>
    [Fact]
    public void ListTheTableWithOneStringPerProcess()
    {
        IReadOnlyList<ProcessFacts> table = [];

        long allocated = Allocations.Of(() => table = registry.Snapshot());

        long names = table.Sum(entry => (22 + (2L * entry.Name.Length) + 7) & ~7L);
        long capacity = (long)BitOperations.RoundUpToPowerOf2((uint)Math.Max(table.Count, 4));
        allocated.ShouldBeLessThanOrEqualTo((32L * table.Count) + names + (16L * capacity) + 4096);
    }

    [Fact]
    public void ReadThisProcesssHandleCount()
    {
        using Process current = Process.GetCurrentProcess();

        int? actual = registry.HandleCountOf(Environment.ProcessId);

        actual.ShouldNotBeNull();
        actual.Value.ShouldBeGreaterThan(0);
        actual.Value.ShouldBeInRange((int)(current.HandleCount * 0.9), (int)(current.HandleCount * 1.1) + 1);
    }

    [Fact]
    public void ReportNoHandleCountForAProcessThatDoesNotExist()
    {
        registry.HandleCountOf(int.MaxValue - 7).ShouldBeNull();
    }

    [Fact]
    public void ReportNothingForAProcessThatDoesNotExist()
    {
        registry.StartTimeOf(int.MaxValue - 7).ShouldBeNull();
        registry.ParentOf(int.MaxValue - 7).ShouldBeNull();
        registry.UsageOf(int.MaxValue - 7).ShouldBeNull();
    }

    [Fact]
    public void NameThisProcessAsTheParentOfAChildItStarted()
    {
        using Process current = Process.GetCurrentProcess();
        using Process child = StartSleeper();
        try
        {
            ProcessParent? parent = registry.ParentOf(child.Id);

            parent.ShouldNotBeNull();
            parent.ProcessId.ShouldBe(Environment.ProcessId);
            parent.Name.ShouldBe(current.ProcessName);
        }
        finally
        {
            child.Kill(entireProcessTree: true);
        }
    }

    [Fact]
    public void ListThisProcessAndAChildItStartedInTheSnapshot()
    {
        using Process current = Process.GetCurrentProcess();
        using Process child = StartSleeper();
        try
        {
            IReadOnlyList<ProcessFacts> snapshot = registry.Snapshot();

            ProcessFacts self = snapshot.Single(process => process.ProcessId == Environment.ProcessId);
            self.Name.ShouldBe(current.ProcessName);
            ProcessFacts sleeper = snapshot.Single(process => process.ProcessId == child.Id);
            sleeper.ParentId.ShouldBe(Environment.ProcessId);
            sleeper.Name.ShouldBe("cmd");
        }
        finally
        {
            child.Kill(entireProcessTree: true);
        }
    }

    [Fact]
    public void MeasureAChildItStarted()
    {
        DateTimeOffset before = TimeProvider.System.GetUtcNow().AddMinutes(-1);
        using Process child = StartSleeper();
        try
        {
            ProcessUsage? usage = registry.UsageOf(child.Id);

            usage.ShouldNotBeNull();
            usage.StartedAt.ShouldBeInRange(before, TimeProvider.System.GetUtcNow().AddMinutes(1));
            usage.StartedAt.Offset.ShouldBe(TimeSpan.Zero);
            usage.PrivateMemory.Bytes.ShouldBeGreaterThan(0);
            usage.Label.ShouldBe("ping");
        }
        finally
        {
            child.Kill(entireProcessTree: true);
        }
    }

    /// <summary>A refresh measures a few hundred processes, so one query must cost well under a millisecond, not the twenty that enumerating the table per query costs.</summary>
    [Fact]
    public void MeasureTwoHundredTimesWithinASecond()
    {
        Stopwatch clock = Stopwatch.StartNew();
        for (int round = 0; round < 200; round++)
        {
            registry.UsageOf(Environment.ProcessId).ShouldNotBeNull();
        }

        clock.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(1));
    }

    /// <summary>Every read opens a handle of its own, so a refresh that measures a few hundred processes must close each one it opens.</summary>
    [Fact]
    public void CloseEveryHandleItOpens()
    {
        registry.UsageOf(Environment.ProcessId).ShouldNotBeNull();
        int before = HandleCounts.Settled();

        for (int round = 0; round < 200; round++)
        {
            registry.StartTimeOf(Environment.ProcessId).ShouldNotBeNull();
            registry.ParentOf(Environment.ProcessId).ShouldNotBeNull();
            registry.UsageOf(Environment.ProcessId).ShouldNotBeNull();
        }

        registry.Snapshot().ShouldNotBeEmpty();

        (HandleCounts.Settled() - before).ShouldBeLessThan(25);
    }

    [Fact]
    public void MeasureThisProcessBelowItsWorkingSet()
    {
        using Process current = Process.GetCurrentProcess();
        ProcessUsage? usage = registry.UsageOf(Environment.ProcessId);

        usage.ShouldNotBeNull();
        usage.PrivateMemory.Bytes.ShouldBeGreaterThan(0);
        usage.PrivateMemory.Bytes.ShouldBeLessThan(current.WorkingSet64);
    }

    [Fact]
    public void EndAProcessCleanlySoItsExitCodeIsZero()
    {
        using Process child = StartWaiter();

        registry.EndCleanly(Confirmed(child));

        child.WaitForExit(TimeSpan.FromSeconds(5)).ShouldBeTrue();
        child.ExitCode.ShouldBe(0);
    }

    /// <summary>Every door to a confirmed process needs a start time read here, so an id the lookup that ends processes throws for never reaches it.</summary>
    [Fact]
    public void ReadNoStartTimeForAnIdOfZeroOrLess()
    {
        registry.StartTimeOf(0).ShouldBeNull();
        registry.StartTimeOf(-1).ShouldBeNull();
    }

    [Fact]
    public void LeaveAProcessThatHasGoneAlone()
    {
        ConfirmedProcess gone = TestProcesses.Confirmed(int.MaxValue - 7, TimeProvider.System.GetUtcNow());

        Should.NotThrow(() => registry.Terminate(gone));
        Should.NotThrow(() => registry.EndCleanly(gone));
    }

    /// <summary>
    /// The System process refuses to be ended by anyone. Only a clean end is aimed at it: ending a tree goes on to the
    /// children of a process that refused, and System's are not this test's to try.
    /// </summary>
    [Fact]
    public void LeaveAProtectedProcessThatRefusesToEndAlone()
    {
        Should.NotThrow(() => registry.EndCleanly(TestProcesses.Confirmed(4, TimeProvider.System.GetUtcNow())));
    }

    [Fact]
    public void EndATreeWhoseChildHasAlreadyGoneWithoutThrowing()
    {
        ProcessStartInfo start = new("cmd.exe", ["/c", "cmd /c exit 0 & ping -n 30 127.0.0.1 > nul"]) { UseShellExecute = false, CreateNoWindow = true };
        using Process parent = Process.Start(start) ?? throw new InvalidOperationException("cmd did not start");

        Should.NotThrow(() => registry.Terminate(Confirmed(parent)));

        parent.WaitForExit(TimeSpan.FromSeconds(5)).ShouldBeTrue();
    }

    private static ConfirmedProcess Confirmed(Process process) => TestProcesses.Confirmed(process.Id, new DateTimeOffset(process.StartTime.ToUniversalTime()));

    /// <summary>A process with no child: ending only it, as a clean end does, leaves no ping running for the collection's checks.</summary>
    private static Process StartWaiter()
    {
        ProcessStartInfo start = new(Path.Combine(Environment.SystemDirectory, "waitfor.exe"), ["/t", "30", "CaderTest" + Guid.NewGuid().ToString("N")]) { UseShellExecute = false, CreateNoWindow = true };

        return Process.Start(start) ?? throw new InvalidOperationException("waitfor did not start");
    }

    private static Process StartSleeper()
    {
        ProcessStartInfo start = new("cmd.exe", ["/c", "ping.exe -n 30 127.0.0.1 > nul"]) { UseShellExecute = false, CreateNoWindow = true };

        return Process.Start(start) ?? throw new InvalidOperationException("cmd did not start");
    }
}