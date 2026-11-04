namespace Source191.Vantage.ReadmeScene;

/// <summary>A pull request's state in GitHub's words (open, merged, closed) or Azure DevOps's (completed, abandoned); open and draft serve both.</summary>
public enum ScenePullRequestState
{
    Open,
    Draft,
    Merged,
    Closed,
    Completed,
    Abandoned,
}
