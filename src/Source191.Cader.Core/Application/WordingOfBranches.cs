using Source191.Cader.Application.Views;
using Source191.Cader.Domain;

namespace Source191.Cader.Application;

/// <summary>The words for a branch's flag: the BRANCH cell, its tone and the reason the pane gives; the column's help is beside the others'.</summary>
public static partial class Wording
{
    /// <summary>The BRANCH cell: the flag's glyph and word, blank when nothing is flagged; a closed enum, so a new flag fails the build here.</summary>
    public static string BranchCell(BranchFlagKind flag) => flag switch
    {
        BranchFlagKind.LeftBehind => "▲ left behind",
        BranchFlagKind.Gone => "✗ gone",
        BranchFlagKind.Closed => "✗ closed",
        BranchFlagKind.NoPullRequest => "· no PR",
        BranchFlagKind.None => string.Empty,
    };

    public static TextTone ToneOf(BranchFlagKind flag) => flag switch
    {
        BranchFlagKind.LeftBehind => TextTone.Red,
        BranchFlagKind.Gone => TextTone.Amber,
        BranchFlagKind.Closed => TextTone.Amber,
        BranchFlagKind.NoPullRequest => TextTone.Grey,
        BranchFlagKind.None => TextTone.Plain,
    };

    /// <summary>The flag's reason in full, for the pane's Branch line; empty when nothing is flagged.</summary>
    public static string BranchReason(WorktreeRow row) => (row.Flag, row.PullRequest) switch
    {
        (BranchFlagKind.LeftBehind, PullRequestLookup.Found found) => $"{Count(row.CommitsAfterMerge, "commit")} after {PullRequestCell(found)}",
        (BranchFlagKind.Gone, _) => "origin no longer has it, as of the last fetch",
        (BranchFlagKind.Closed, PullRequestLookup.Found found) => $"{PullRequestCell(found)} without merging",
        (BranchFlagKind.NoPullRequest, _) => "no pull request on its host",
        _ => string.Empty,
    };
}
