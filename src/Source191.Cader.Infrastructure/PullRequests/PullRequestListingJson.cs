using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Text.Json;

namespace Source191.Cader.Adapters.PullRequests;

/// <summary>
/// Walks a host's JSON array of pull requests, one row at a time. A row the adapter leaves out (null) or that lacks a
/// property it reads is skipped, as a link that is not https is; text that is not the expected JSON fails the listing.
/// </summary>
internal static class PullRequestListingJson
{
    public static PullRequestListing Rows(string json, Func<JsonElement, BranchPullRequest?> row)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            List<BranchPullRequest> rows = [.. document.RootElement.EnumerateArray().Select(item => Skipping(item, row)).OfType<BranchPullRequest>()];

            return new PullRequestListing.Listed(rows);
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or FormatException)
        {
            return new PullRequestListing.Unavailable(PullRequestGap.Failed);
        }
    }

    /// <summary>
    /// The commit id in <paramref name="holder"/>'s <paramref name="property"/>, or <see cref="HeadCommit.NotRecorded"/> when
    /// the holder is not an object, or the property is missing, not text or not a commit id: the pull request is still listed.
    /// </summary>
    public static HeadCommit HeadCommitIn(JsonElement holder, string property) =>
        holder.ValueKind == JsonValueKind.Object
        && holder.TryGetProperty(property, out JsonElement value)
        && value.ValueKind == JsonValueKind.String
        && CommitId.TryParse(value.GetString(), out CommitId? commit)
            ? new HeadCommit.Known(commit)
            : new HeadCommit.NotRecorded();

    private static BranchPullRequest? Skipping(JsonElement item, Func<JsonElement, BranchPullRequest?> row)
    {
        try
        {
            return row(item);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }
}