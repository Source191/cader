namespace Source191.Cader.Domain;

/// <summary>The settings dialog's fields as typed, before any rule is applied.</summary>
public sealed record SettingsText(string Roots, string WorktreeRoot, EditorPreset Preset, string Executable, string Arguments, WorktreeBase Base, bool CheckForUpdates, LaunchMode Place, bool DiagnosticLog, BeforeBranching BeforeBranching);