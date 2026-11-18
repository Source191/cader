using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Diagnostics;

namespace Source191.Cader.Application;

/// <summary>
/// The Claude accounts Cader has seen logged in and who is logged in now, never waited for by a row: Find answers from
/// what has landed, and RefreshStaleAsync runs <c>claude auth status</c> when the last run started a minute or more ago
/// and none is running. A login from an organisation not yet remembered is remembered under the lowest free number; a
/// remembered one seen with another email or name is updated and keeps its label. Changes and renames are written to
/// the store one at a time.
/// </summary>
public sealed class AccountCatalogue(IClaudeCli claude, IKnownAccounts store, TimeProvider clock) : IDisposable
{
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    private readonly SharedSnapshot<Seen> seen = new(new Seen([], new ClaudeLogin.NotRead()));
    private readonly SemaphoreSlim writes = new(1);
    private readonly Lock gate = new();
    private Task running = Task.CompletedTask;
    private DateTimeOffset lastStarted = DateTimeOffset.MinValue;

    /// <summary>Every remembered account with whether it is logged in now, in label order (<c>2</c> before <c>10</c>).</summary>
    public IReadOnlyList<RowAccount.Known> Accounts
    {
        get
        {
            Seen now = seen.Value;

            return [.. now.Accounts.Order(LabelOrder).Select(account => new RowAccount.Known(account, IsLoggedIn(now.Login, account)))];
        }
    }

    public async Task LoadAsync()
    {
        IReadOnlyList<KnownAccount> remembered = await store.ReadAccountsAsync();
        seen.Replace(seen.Value with { Accounts = remembered });
    }

    public RowAccount Find(SessionOwner owner)
    {
        Seen now = seen.Value;

        return owner switch
        {
            SessionOwner.Recorded recorded => now.Accounts.FirstOrDefault(account => account.Organisation == recorded.Organisation) is KnownAccount account
                ? new RowAccount.Known(account, IsLoggedIn(now.Login, account))
                : new RowAccount.Unseen(),
            SessionOwner.NotRecorded => new RowAccount.Unrecorded(),
            _ => throw new UnreachableException(),
        };
    }

    public Task RefreshStaleAsync()
    {
        lock (gate)
        {
            DateTimeOffset now = clock.GetUtcNow();
            if (!running.IsCompleted || now - lastStarted < Window)
            {
                return running;
            }

            lastStarted = now;
            running = RefreshAsync();

            return running;
        }
    }

    /// <summary>True when the label changed and was written; false for the same label or an account no longer remembered.</summary>
    public async Task<bool> RenameAsync(OrganisationId organisation, AccountLabel label)
    {
        await writes.WaitAsync();
        try
        {
            Seen now = seen.Value;
            if (now.Accounts.FirstOrDefault(account => account.Organisation == organisation) is not KnownAccount account || account.Label == label)
            {
                return false;
            }

            Seen renamed = now with { Accounts = [.. now.Accounts.Select(each => each == account ? each with { Label = label } : each)] };
            seen.Replace(renamed);
            await store.WriteAccountsAsync(renamed.Accounts);

            return true;
        }
        finally
        {
            writes.Release();
        }
    }

    public void Dispose() => writes.Dispose();

    private async Task RefreshAsync()
    {
        ClaudeLogin login = await claude.LoginAsync(CancellationToken.None);
        await writes.WaitAsync();
        try
        {
            Seen now = seen.Value;
            IReadOnlyList<KnownAccount> accounts = Remember(now.Accounts, login);
            seen.Replace(new Seen(accounts, login));
            if (!ReferenceEquals(accounts, now.Accounts))
            {
                await store.WriteAccountsAsync(accounts);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Nothing awaits a refresh started by a row; the accounts stay in memory and the next change writes them.
        }
        finally
        {
            writes.Release();
        }
    }

    /// <summary>The same list when nothing changed, so the store is written only for a change.</summary>
    private static IReadOnlyList<KnownAccount> Remember(IReadOnlyList<KnownAccount> accounts, ClaudeLogin login)
    {
        if (login is not ClaudeLogin.LoggedIn current)
        {
            return accounts;
        }

        if (accounts.FirstOrDefault(account => account.Organisation == current.Organisation) is not KnownAccount known)
        {
            return [.. accounts, new KnownAccount(current.Organisation, current.Email, current.OrganisationName, AccountLabels.NextFree(accounts.Select(account => account.Label)))];
        }

        KnownAccount updated = known with { Email = current.Email, OrganisationName = current.OrganisationName };

        return updated == known ? accounts : [.. accounts.Select(account => account == known ? updated : account)];
    }

    private static bool IsLoggedIn(ClaudeLogin login, KnownAccount account) => login is ClaudeLogin.LoggedIn current && current.Organisation == account.Organisation;

    private static readonly IComparer<KnownAccount> LabelOrder = Comparer<KnownAccount>.Create((first, second) => NumberAwareOrder.Compare(first.Label.Value, second.Label.Value));

    private sealed record Seen(IReadOnlyList<KnownAccount> Accounts, ClaudeLogin Login);
}