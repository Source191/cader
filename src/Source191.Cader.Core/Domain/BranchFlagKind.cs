namespace Source191.Cader.Domain;

/// <summary>The most urgent thing to know about a branch, as <see cref="BranchFlags.Of"/> decides it; <c>Wording</c> gives each its glyph and words.</summary>
public enum BranchFlagKind
{
    /// <summary>Nothing to flag, or the pull request lookup could not say.</summary>
    None,

    /// <summary>The shown pull request merged, and the branch has commits its recorded head does not contain.</summary>
    LeftBehind,

    /// <summary>The upstream has been deleted on origin, as of the last fetch.</summary>
    Gone,

    /// <summary>The shown pull request was closed or abandoned without merging.</summary>
    Closed,

    /// <summary>The host listed no pull request for the branch.</summary>
    NoPullRequest,
}
