namespace Source191.Cader.Application.Detail;

/// <summary>How a tab's detail pane reads its selected row: the most lines its detail can fill, the row's key across refreshes, and the pane's content for the lines it shows.</summary>
public interface IDetailSource<in TRow>
{
    /// <summary>The most lines under the divider this tab's detail can fill, which the pane grows no taller than.</summary>
    int MostRows { get; }

    object KeyOf(TRow row);

    /// <param name="row">The selected row.</param>
    /// <param name="lines">The lines the pane shows under its divider.</param>
    /// <param name="cancellationToken">Cancels the reads still running.</param>
    Task<DetailContent> ReadAsync(TRow row, int lines, CancellationToken cancellationToken);
}