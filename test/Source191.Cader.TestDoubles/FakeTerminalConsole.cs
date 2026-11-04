using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.TestDoubles;

/// <summary>Records what the loop asks of the terminal Cader runs in.</summary>
public sealed class FakeTerminalConsole : ITerminalConsole
{
    /// <summary>The colour of each time the own tab was shown coloured.</summary>
    public List<TabColour> Coloured { get; } = [];

    /// <summary>How often the own tab was titled alone.</summary>
    public int TitledOnly { get; private set; }

    public int Cleared { get; private set; }

    public int Recovered { get; private set; }

    /// <summary>The return hint's showing and removal, in order; a test may add other fakes' events to put them in sequence.</summary>
    public List<string> Events { get; } = [];

    public void ShowOwnTab(TabColour colour)
    {
        Coloured.Add(colour);
    }

    public void ShowOwnTab()
    {
        TitledOnly++;
    }

    public void ClearOwnTab()
    {
        Cleared++;
    }

    public void ShowReturnHint(string hint)
    {
        Events.Add($"return hint shown: {hint}");
    }

    public void RemoveReturnHint()
    {
        Events.Add("return hint removed");
    }

    public void RecoverAfterChild()
    {
        Recovered++;
    }
}