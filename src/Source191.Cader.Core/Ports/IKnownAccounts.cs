namespace Source191.Cader.Ports;

/// <summary>The accounts Cader remembers (accounts.json): none when there is no file or it cannot be read.</summary>
public interface IKnownAccounts
{
    Task<IReadOnlyList<KnownAccount>> ReadAccountsAsync();

    Task WriteAccountsAsync(IReadOnlyList<KnownAccount> accounts);
}