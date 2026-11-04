namespace Source191.Vantage.ReadmeScene;

/// <summary>The paths the scene's programs answer for: this program standing in for Claude Code, and the gh and az the pull-request adapters are told about (never run).</summary>
public sealed record SceneExecutables(string Claude, string GitHub, string AzureDevOps);
