namespace Source191.Cader.Application.Views;

/// <summary>The fact behind a table's title: <see cref="Noun"/> is singular (session), <see cref="Filter"/> empty when none is set and <see cref="Selected"/> null when no row is.</summary>
public sealed record TableTitle(string Noun, int Shown, int Total, string Filter, string? Selected)
{
    /// <summary>The part of <see cref="Total"/> that is of a second kind, counted apart after the noun's.</summary>
    public TitleCount Apart { get; init; } = TitleCount.None;
}