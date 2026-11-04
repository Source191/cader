namespace Source191.Cader.Application.Views;

public sealed record ConfigRowsLoaded(IReadOnlyList<ConfigRow> Rows) : ViewInput;