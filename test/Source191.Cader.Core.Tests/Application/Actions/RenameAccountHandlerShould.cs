using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Adapters.Claude;
using Source191.Cader.Application;
using Source191.Cader.Application.Actions;
using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Actions;

public sealed class RenameAccountHandlerShould : IDisposable
{
    private static readonly KnownAccount First = new(OrganisationId.From("6f1c2a8e-4b7d-4c1e-9a3f-2d5e8b0c7a91"), "me@northwind.example", "Northwind Traders", AccountLabel.From("1"));

    private readonly InMemoryState state = new();
    private readonly AccountCatalogue accounts;

    public RenameAccountHandlerShould()
    {
        accounts = new AccountCatalogue(new ClaudeCli(new FakeProcessRunner(), @"C:\Users\me\.local\bin\claude.exe"), state, new FakeTimeProvider(Fixtures.Noon));
    }

    [Fact]
    public async Task RenameTheAccountAndSaveIt()
    {
        await state.WriteAccountsAsync([First]);
        await accounts.LoadAsync();

        Outcome outcome = await new RenameAccountHandler(accounts).HandleAsync(new RenameAccount(First, AccountLabel.From("work")), CancellationToken.None);

        outcome.ShouldBe(new RenameAccountOutcome.Renamed(AccountLabel.From("1"), AccountLabel.From("work")));
        (await state.ReadAccountsAsync()).ShouldBe([First with { Label = AccountLabel.From("work") }]);
    }

    [Fact]
    public async Task SayTheLabelIsUnchangedForTheSameLabel()
    {
        await state.WriteAccountsAsync([First]);
        await accounts.LoadAsync();

        Outcome outcome = await new RenameAccountHandler(accounts).HandleAsync(new RenameAccount(First, AccountLabel.From("1")), CancellationToken.None);

        outcome.ShouldBe(new RenameAccountOutcome.Unchanged(AccountLabel.From("1")));
        state.AccountWrites.ShouldBe(1);
    }

    public void Dispose() => accounts.Dispose();
}
