using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Adapters.Claude;
using Source191.Cader.Application;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application;

public sealed class AccountCatalogueShould : IDisposable
{
    private const string Claude = @"C:\Users\me\.local\bin\claude.exe";

    private static readonly OrganisationId Northwind = OrganisationId.From("6f1c2a8e-4b7d-4c1e-9a3f-2d5e8b0c7a91");
    private static readonly OrganisationId Contoso = OrganisationId.From("0b9d4e2f-7a6c-4f3e-8d1b-5c2a9e7f4b60");
    private static readonly KnownAccount Work = new(Northwind, "me@northwind.example", "Northwind Traders", AccountLabel.From("work"));
    private static readonly KnownAccount Home = new(Contoso, "me@home.example", "Personal", AccountLabel.From("2"));

    private readonly FakeProcessRunner processes = new();
    private readonly InMemoryState state = new();
    private readonly FakeTimeProvider clock = new(new DateTimeOffset(2026, 11, 18, 12, 0, 0, TimeSpan.Zero));
    private readonly AccountCatalogue catalogue;

    public AccountCatalogueShould()
    {
        catalogue = new AccountCatalogue(new ClaudeCli(processes, Claude), state, clock);
    }

    [Fact]
    public void FindNoAccountForASessionWithNoRecordedOwner()
    {
        catalogue.Find(new SessionOwner.NotRecorded()).ShouldBe(new RowAccount.Unrecorded());
    }

    [Fact]
    public async Task FindAnOwnerNotYetSeenLoggedInAsUnseen()
    {
        await state.WriteAccountsAsync([Work]);
        await catalogue.LoadAsync();

        catalogue.Find(new SessionOwner.Recorded(Contoso)).ShouldBe(new RowAccount.Unseen());
    }

    [Fact]
    public async Task FindARememberedOwnerNotLoggedInNow()
    {
        await state.WriteAccountsAsync([Work]);
        await catalogue.LoadAsync();

        catalogue.Find(new SessionOwner.Recorded(Northwind)).ShouldBe(new RowAccount.Known(Work, LoggedInNow: false));
    }

    [Fact]
    public async Task FindTheOwnerLoggedInNow()
    {
        await state.WriteAccountsAsync([Work]);
        await catalogue.LoadAsync();
        LogIn(Northwind, "me@northwind.example", "Northwind Traders");

        await catalogue.RefreshStaleAsync();

        catalogue.Find(new SessionOwner.Recorded(Northwind)).ShouldBe(new RowAccount.Known(Work, LoggedInNow: true));
    }

    [Fact]
    public async Task RememberTheFirstLoginAsOne()
    {
        LogIn(Northwind, "me@northwind.example", "Northwind Traders");

        await catalogue.RefreshStaleAsync();

        KnownAccount first = new(Northwind, "me@northwind.example", "Northwind Traders", AccountLabel.From("1"));
        (await state.ReadAccountsAsync()).ShouldBe([first]);
        catalogue.Accounts.ShouldBe([new RowAccount.Known(first, LoggedInNow: true)]);
    }

    [Fact]
    public async Task RememberAnotherLoginUnderTheLowestFreeNumber()
    {
        await state.WriteAccountsAsync([Work]);
        await catalogue.LoadAsync();
        LogIn(Contoso, "me@home.example", "Personal");

        await catalogue.RefreshStaleAsync();

        (await state.ReadAccountsAsync()).ShouldBe([Work, Home with { Label = AccountLabel.From("1") }]);
    }

    [Fact]
    public async Task UpdateAChangedEmailAndKeepTheLabel()
    {
        await state.WriteAccountsAsync([Work]);
        await catalogue.LoadAsync();
        LogIn(Northwind, "orders@northwind.example", "Northwind Traders");

        await catalogue.RefreshStaleAsync();

        (await state.ReadAccountsAsync()).ShouldBe([Work with { Email = "orders@northwind.example" }]);
    }

    [Fact]
    public async Task WriteNothingWhenTheLoginIsAlreadyRemembered()
    {
        await state.WriteAccountsAsync([Work]);
        await catalogue.LoadAsync();
        LogIn(Northwind, "me@northwind.example", "Northwind Traders");

        await catalogue.RefreshStaleAsync();

        state.AccountWrites.ShouldBe(1);
    }

    [Fact]
    public async Task AskForTheLoginAtMostOnceAMinute()
    {
        LogIn(Northwind, "me@northwind.example", "Northwind Traders");
        await catalogue.RefreshStaleAsync();

        clock.Advance(TimeSpan.FromSeconds(59));
        await catalogue.RefreshStaleAsync();
        processes.CapturedRequests.Count.ShouldBe(1);

        clock.Advance(TimeSpan.FromSeconds(1));
        await catalogue.RefreshStaleAsync();
        processes.CapturedRequests.Count.ShouldBe(2);
    }

    [Fact]
    public async Task RunOneLoginAtATime()
    {
        TaskCompletionSource release = new();
        processes.WhileRunning = _ => release.Task;
        LogIn(Northwind, "me@northwind.example", "Northwind Traders");

        Task first = catalogue.RefreshStaleAsync();
        clock.Advance(TimeSpan.FromMinutes(2));
        Task second = catalogue.RefreshStaleAsync();
        release.SetResult();
        await Task.WhenAll(first, second);

        processes.CapturedRequests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task KnowNoCurrentLoginAfterALogOut()
    {
        await state.WriteAccountsAsync([Work]);
        await catalogue.LoadAsync();
        LogIn(Northwind, "me@northwind.example", "Northwind Traders");
        await catalogue.RefreshStaleAsync();

        processes.AnswerAll(new ProcessResult(0, """{"loggedIn":false}""", string.Empty));
        clock.Advance(TimeSpan.FromMinutes(1));
        await catalogue.RefreshStaleAsync();

        catalogue.Find(new SessionOwner.Recorded(Northwind)).ShouldBe(new RowAccount.Known(Work, LoggedInNow: false));
    }

    [Fact]
    public async Task KnowNoCurrentLoginWhenClaudeCannotBeRun()
    {
        await state.WriteAccountsAsync([Work]);
        await catalogue.LoadAsync();
        processes.AnswerAll(new ProcessResult(1, string.Empty, "error"));

        await catalogue.RefreshStaleAsync();

        catalogue.Accounts.ShouldBe([new RowAccount.Known(Work, LoggedInNow: false)]);
    }

    [Fact]
    public async Task ListTheAccountsInNumberAwareLabelOrder()
    {
        KnownAccount ten = Home with { Organisation = OrganisationId.From("11111111-1111-4111-8111-111111111111"), Label = AccountLabel.From("10") };
        await state.WriteAccountsAsync([ten, Work, Home]);
        await catalogue.LoadAsync();

        catalogue.Accounts.Select(known => known.Account.Label.Value).ShouldBe(["2", "10", "work"]);
    }

    [Fact]
    public async Task RenameAnAccount()
    {
        await state.WriteAccountsAsync([Home]);
        await catalogue.LoadAsync();

        (await catalogue.RenameAsync(Contoso, AccountLabel.From("personal"))).ShouldBeTrue();

        (await state.ReadAccountsAsync()).ShouldBe([Home with { Label = AccountLabel.From("personal") }]);
        catalogue.Find(new SessionOwner.Recorded(Contoso)).ShouldBe(new RowAccount.Known(Home with { Label = AccountLabel.From("personal") }, LoggedInNow: false));
    }

    [Fact]
    public async Task WriteNothingForTheSameLabel()
    {
        await state.WriteAccountsAsync([Home]);
        await catalogue.LoadAsync();

        (await catalogue.RenameAsync(Contoso, AccountLabel.From("2"))).ShouldBeFalse();

        state.AccountWrites.ShouldBe(1);
    }

    [Fact]
    public async Task RenameNothingForAnAccountNotRemembered()
    {
        (await catalogue.RenameAsync(Contoso, AccountLabel.From("personal"))).ShouldBeFalse();

        state.AccountWrites.ShouldBe(0);
    }

    public void Dispose() => catalogue.Dispose();

    private void LogIn(OrganisationId organisation, string email, string name)
    {
        processes.AnswerAll(new ProcessResult(0, $$"""{"loggedIn":true,"email":"{{email}}","orgId":"{{organisation.Value}}","orgName":"{{name}}"}""", string.Empty));
    }
}