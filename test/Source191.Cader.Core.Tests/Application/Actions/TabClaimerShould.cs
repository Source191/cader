using Source191.Cader.Application.Actions;
using Source191.Cader.Domain;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Actions;

public sealed class TabClaimerShould
{
    private readonly InMemoryState store = new();
    private readonly FakeProcessRegistry registry = new();

    private TabClaimer Claimer => new(store, registry);

    private static TabRegistration Pending() => TabRegistration.Pending(new TabOwner.ForSession(SessionId.From("sess-1")), TabPalette.Default[0], Fixtures.Noon);

    [Fact]
    public async Task StampTheRegistrationWithTheShellAndItsStart()
    {
        TabRegistration pending = Pending();
        await store.WriteTabAsync(pending);
        registry.Running(4242, Fixtures.Noon.AddSeconds(2));

        await Claimer.ClaimAsync(pending.Id, processId: 4242);

        (await store.ReadTabsAsync()).Single().Claim.ShouldBe(new TabClaim.Claimed(4242, Fixtures.Noon.AddSeconds(2)));
    }

    [Fact]
    public async Task ClaimNothingForARegistrationThatIsNotThere()
    {
        TabRegistration other = Pending();
        await store.WriteTabAsync(other);
        registry.Running(4242, Fixtures.Noon.AddSeconds(2));

        await Claimer.ClaimAsync(Guid.NewGuid(), processId: 4242);

        (await store.ReadTabsAsync()).ShouldBe([other]);
    }

    [Fact]
    public async Task ClaimNothingForAShellThatIsNotRunning()
    {
        TabRegistration pending = Pending();
        await store.WriteTabAsync(pending);

        await Claimer.ClaimAsync(pending.Id, processId: 4242);

        (await store.ReadTabsAsync()).ShouldBe([pending]);
    }

    [Fact]
    public async Task ClaimNothingWhenThereIsNoRegistrationAtAll()
    {
        await Claimer.ClaimAsync(Guid.NewGuid(), processId: 4242);

        (await store.ReadTabsAsync()).ShouldBeEmpty();
    }
}