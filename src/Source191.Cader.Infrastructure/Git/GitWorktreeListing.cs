using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Adapters.Git;

/// <summary>
/// Parses <see href="https://git-scm.com/docs/git-worktree">"git worktree list --porcelain"</see>: blank-line separated blocks of "worktree", "HEAD", then
/// "branch" or "detached", with optional "locked [reason]" and "prunable reason" lines.
/// </summary>
public static class GitWorktreeListing
{
    /// <summary>Walks the output's lines as spans, so only the values an entry keeps become strings; an empty line closes a block.</summary>
    public static IReadOnlyList<GitWorktreeEntry> Parse(string porcelain)
    {
        List<GitWorktreeEntry> entries = [];
        Block block = new();
        foreach (ReadOnlySpan<char> line in porcelain.AsSpan().EnumerateLines())
        {
            block = line.Trim().IsEmpty ? Closed(block, entries) : block.Read(line.Trim());
        }

        Closed(block, entries);

        return entries;
    }

    /// <summary>Adds the block's entry when it saw a line, the first one as the main worktree, and answers a fresh block.</summary>
    private static Block Closed(Block block, List<GitWorktreeEntry> entries)
    {
        if (!block.SawALine)
        {
            return block;
        }

        entries.Add(block.Entry(isMain: entries.Count == 0));

        return new Block();
    }

    /// <summary>One block's lines read so far.</summary>
    private sealed class Block
    {
        private const string RefsHeads = "refs/heads/";

        private string path = string.Empty;
        private string commit = string.Empty;
        private Head head = new Head.Unknown();
        private WorktreeLock locked = new WorktreeLock.Unlocked();
        private bool prunable;

        public bool SawALine { get; private set; }

        public Block Read(ReadOnlySpan<char> line)
        {
            SawALine = true;
            int space = line.IndexOf(' ');
            ReadOnlySpan<char> key = space < 0 ? line : line[..space];
            ReadOnlySpan<char> value = space < 0 ? [] : line[(space + 1)..];
            switch (key)
            {
                case "worktree":
                    path = string.Create(value.Length, value, static (destination, source) => source.Replace(destination, '/', Path.DirectorySeparatorChar));
                    break;
                case "HEAD":
                    commit = value.ToString();
                    break;
                case "branch":
                    head = HeadOn(value);
                    break;
                case "detached":
                    head = new Head.Detached();
                    break;
                case "locked":
                    locked = new WorktreeLock.Locked(value.ToString());
                    break;
                case "prunable":
                    prunable = true;
                    break;
            }

            return this;
        }

        public GitWorktreeEntry Entry(bool isMain) => new(path, commit, head, isMain, locked, prunable);

        /// <summary>The branch a "branch" line names, without its refs/heads/ prefix; Unknown when that is not a branch name.</summary>
        private static Head HeadOn(ReadOnlySpan<char> reference)
        {
            ReadOnlySpan<char> branch = reference.StartsWith(RefsHeads, StringComparison.Ordinal) ? reference[RefsHeads.Length..] : reference;

            return BranchName.TryParse(branch.ToString(), out BranchName? name) ? new Head.OnBranch(name) : new Head.Unknown();
        }
    }
}