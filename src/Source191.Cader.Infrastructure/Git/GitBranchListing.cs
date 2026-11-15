using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Globalization;

namespace Source191.Cader.Adapters.Git;

/// <summary>
/// Parses <see href="https://git-scm.com/docs/git-for-each-ref">git for-each-ref</see> over refs/heads and
/// refs/remotes/origin with <see cref="Format"/>: one ref a line, its fields NUL-separated, which no ref name or email
/// holds. A ref whose fields cannot be read is left out, and so is origin's HEAD, a symbolic ref.
/// </summary>
internal static class GitBranchListing
{
    /// <summary>The ref, its upstream, the upstream's state ("[gone]" once a fetch with prune has deleted it), the tip's commit time and author email, and the symbolic ref's target.</summary>
    public const string Format = "--format=%(refname)%00%(upstream:short)%00%(upstream:track)%00%(committerdate:iso-strict)%00%(authoremail)%00%(symref)";

    private const string LocalPrefix = "refs/heads/";
    private const string OriginPrefix = "refs/remotes/origin/";

    /// <summary>The email is compared here, ordinal ignoring case, and answered only as each branch's Mine; an empty one matches nothing.</summary>
    public static IReadOnlyList<GitBranch> Parse(string output, string userEmail)
    {
        List<GitBranch> branches = [.. output.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line => Branch(line.TrimEnd('\r').Split('\0'), userEmail)).OfType<GitBranch>()];
        HashSet<BranchName> local = [.. branches.OfType<GitBranch.Local>().Select(branch => branch.Name)];

        return [.. branches.Where(branch => branch is GitBranch.Local || !local.Contains(branch.Name))];
    }

    private static GitBranch? Branch(string[] fields, string userEmail)
    {
        if (fields is not [string name, string upstream, string track, string tipAt, string email, ""]
            || !DateTimeOffset.TryParse(tipAt, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset at))
        {
            return null;
        }

        bool mine = userEmail.Length > 0 && string.Equals(email.Trim('<', '>'), userEmail, StringComparison.OrdinalIgnoreCase);

        return name switch
        {
            _ when name.StartsWith(LocalPrefix, StringComparison.Ordinal) && BranchName.TryParse(name[LocalPrefix.Length..], out BranchName? branch) => new GitBranch.Local(branch, UpstreamOf(upstream, track), at, mine),
            _ when name.StartsWith(OriginPrefix, StringComparison.Ordinal) && BranchName.TryParse(name[OriginPrefix.Length..], out BranchName? branch) => new GitBranch.OriginOnly(branch, at, mine),
            _ => null,
        };
    }

    private static Upstream UpstreamOf(string upstream, string track) => upstream switch
    {
        _ when track == "[gone]" => new Upstream.Gone(),
        _ when RemoteBranch.TryParse(upstream, out RemoteBranch? remote) => new Upstream.Tracking(remote),
        _ => new Upstream.None(),
    };
}