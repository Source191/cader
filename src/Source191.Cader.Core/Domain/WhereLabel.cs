namespace Source191.Cader.Domain;

/// <summary>
/// Where a worktree's folder is, under the heading that names its repository: the folder's part after the repository (the
/// slug of a folder named <c>repository-slug</c>, or the path inside the repository), else the whole folder label. The
/// branch is the detail pane's, never the label's.
/// </summary>
public static class WhereLabel
{
    /// <summary>The slug or inner path when the label starts with the repository and a dash or a backslash, else the whole label.</summary>
    public static string PlaceOf(string repository, string label)
    {
        string[] prefixes = [repository + "-", repository + "\\"];

        return prefixes
            .Where(prefix => label.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(prefix => label[prefix.Length..])
            .FirstOrDefault(label);
    }
}
