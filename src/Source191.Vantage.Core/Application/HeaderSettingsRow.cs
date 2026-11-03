using Source191.Vantage.Domain;

namespace Source191.Vantage.Application;

/// <summary>
/// The console's settings row above the tabs: Vantage's version, the update status and Vantage's own memory (zero when
/// the process cannot be measured), each read at every call, then Claude Code's version.
/// </summary>
public sealed class HeaderSettingsRow(ToolVersion vantage, ClaudeCodeVersion claude, UpdateCheck updates, IProcessRegistry processes, int processId)
{
    /// <summary>One row; the width left after the icon is accepted for <c>ScreenContext.SettingsRows</c> and not needed.</summary>
    public IReadOnlyList<IReadOnlyList<TextPart>> Rows(int width) =>
        [Wording.SettingsRow(vantage, claude, updates.Status, processes.UsageOf(processId)?.PrivateMemory ?? MemorySize.Zero)];
}