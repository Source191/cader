using Source191.Cader.Application;
using Source191.Cader.Application.Detail;
using Source191.Cader.Application.Views;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Source191.Cader.Tui;

/// <summary>
/// A tab's detail pane's height, from a key, a drag of its divider or the window: each goes through <see cref="PaneHeight"/> and
/// <see cref="Apply"/>, which anchors the pane to the tab's bottom and stops the table short of it. The height the user wants is
/// <see cref="SettlingPaneHeights"/>'s, saved once it settles; the height shown is that within the room the window leaves, so a
/// short terminal never changes what is saved. A drag leaves the pane's position absolute and ignores any limit, so the height
/// it reports is fitted and re-anchored from a zero timeout, after Terminal.Gui's own write (terminal-gui.md, 20 November 2026).
/// </summary>
internal sealed class PaneSizing
{
    private readonly int most;
    private readonly Place place;
    private readonly IApplication app;
    private readonly Action resized;

    /// <param name="most">The most lines the tab's detail can fill.</param>
    /// <param name="place">The tab, its heights and the views the pane sits among.</param>
    /// <param name="app">The application whose loop runs the re-anchoring after a drag.</param>
    /// <param name="resized">Called when the lines shown change, so the detail is read again for them.</param>
    public PaneSizing(int most, Place place, IApplication app, Action resized)
    {
        this.most = most;
        this.place = place;
        this.app = app;
        this.resized = resized;
        Apply(PaneHeight.Fit(place.Heights.Of(place.Tab), most, Room));
        place.TabView.FrameChanged += (_, _) => Show(PaneHeight.Fit(place.Heights.Of(place.Tab), most, Room));
        Pane.FrameChanged += (_, frame) => Dragged(frame.Value.Height);
    }

    public DetailPane Pane { get; } = new();

    /// <summary>The lines shown under the divider.</summary>
    public int Shown { get; private set; }

    /// <summary>The lines the tab leaves the pane; any number until the tab has been laid out.</summary>
    private int Room => place.TabView.Viewport.Height > 0 ? PaneHeight.Room(place.TabView.Viewport.Height) : int.MaxValue;

    public void Resize(int step) => Want(PaneHeight.Step(Shown, step, most, Room));

    /// <summary>A frame of another height than the one applied is a drag; fitted and anchored once Terminal.Gui has written its own.</summary>
    private void Dragged(int rows)
    {
        if (rows == Shown + PaneHeight.DividerRows)
        {
            return;
        }

        app.AddTimeout(TimeSpan.Zero, () =>
        {
            Want(PaneHeight.Fit(rows - PaneHeight.DividerRows, most, Room));
            return false;
        });
    }

    private void Want(int lines)
    {
        place.Heights.Changed(place.Tab, lines);
        Show(lines);
    }

    private void Show(int lines)
    {
        bool changed = lines != Shown;
        Apply(lines);
        if (changed)
        {
            resized();
        }
    }

    private void Apply(int lines)
    {
        Shown = lines;
        int rows = lines + PaneHeight.DividerRows;
        Pane.Y = Pos.AnchorEnd(rows);
        Pane.Height = rows;
        place.Table.Height = Dim.Fill(rows);
    }

    /// <summary>Where a pane sits: its tab, the heights wanted for each tab, the table above it and the tab's view around both.</summary>
    public sealed record Place(ConsoleTab Tab, SettlingPaneHeights Heights, TableView Table, View TabView);
}