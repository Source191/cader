using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Globalization;
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

    /// <summary>
    /// When a finished pull request finished, from <paramref name="item"/>'s <paramref name="property"/> read as an
    /// invariant time in UTC, or <see cref="RecordedTime.NotRecorded"/> for an open one, or when the property is missing,
    /// not text or not a time: the pull request is still listed and never left out as an earlier branch's.
    /// </summary>
    public static RecordedTime FinishedIn(JsonElement item, string property, PullRequestState state) =>
        !state.IsOpen()
        && item.TryGetProperty(property, out JsonElement value)
        && value.ValueKind == JsonValueKind.String
        && DateTimeOffset.TryParse(value.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset finished)
            ? new RecordedTime.At(finished)
            : new RecordedTime.NotRecorded();

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