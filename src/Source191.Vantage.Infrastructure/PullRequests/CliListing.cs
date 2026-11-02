using Source191.Vantage.Domain;
using Source191.Vantage.Ports;
using System.ComponentModel;

namespace Source191.Vantage.Adapters.PullRequests;

/// <summary>The shell both pull-request adapters share: run a host's CLI, read a CLI that cannot start as missing, and let the adapter read what it answered.</summary>
internal static class CliListing
{
    public static async Task<PullRequestListing> RunAsync(IProcessRunner processes, ProcessRequest request, Func<ProcessResult, PullRequestListing> read, CancellationToken cancellationToken)
    {
        ProcessResult result;
        try
        {
            result = await processes.RunAsync(request, cancellationToken);
        }
        catch (Win32Exception)
        {
            return new PullRequestListing.Unavailable(PullRequestGap.CliMissing);
        }

        return read(result);
    }
}