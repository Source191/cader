using Source191.Vantage.Domain;
using Source191.Vantage.TestDoubles;

namespace Source191.Vantage.Core.Tests.TestDoubles;

public sealed class FakeProcessRegistryShould
{
    private static readonly DateTimeOffset Noon = new(2026, 10, 23, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void EndTheTreeUnderATerminatedProcess()
    {
        FakeProcessRegistry registry = new FakeProcessRegistry()
            .InTree(100, "pwsh", 1, Noon)
            .InTree(101, "claude", 100, Noon)
            .InTree(102, "node", 101, Noon);

        ConfirmedProcess.TryConfirmClaim(100, Noon, Noon, out ConfirmedProcess? process).ShouldBeTrue();
        registry.Terminate(process);

        registry.Terminated.ShouldBe([100]);
        registry.StartTimeOf(101).ShouldBeNull();
        registry.StartTimeOf(102).ShouldBeNull();
        registry.Snapshot().Select(facts => facts.ProcessId).ShouldNotContain(102);
    }

    [Fact]
    public void ListARunningProcessNotInTheTree()
    {
        FakeProcessRegistry registry = new FakeProcessRegistry().Running(4242, Noon);

        ProcessFacts facts = registry.Snapshot().Single();

        (facts.ProcessId, facts.ParentId, facts.Name).ShouldBe((4242, 0, string.Empty));
    }
}