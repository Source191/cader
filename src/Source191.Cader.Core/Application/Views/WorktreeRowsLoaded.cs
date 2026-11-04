namespace Source191.Cader.Application.Views;

public sealed record WorktreeRowsLoaded(IReadOnlyList<WorktreeRow> Rows) : ViewInput;