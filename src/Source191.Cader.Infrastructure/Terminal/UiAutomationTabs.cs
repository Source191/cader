using Interop.UIAutomationClient;
using Source191.Cader.Domain;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Source191.Cader.Adapters.Terminal;

/// <summary>
/// Selects a Windows Terminal tab by its title through
/// <see href="https://learn.microsoft.com/windows/win32/winauto/entry-uiauto-win32">UI Automation</see>, because the
/// <see href="https://learn.microsoft.com/windows/terminal/command-line-arguments">terminal's own command line</see> can
/// only focus a tab by index. The console titles the tabs it opens with the session name and suppresses title changes,
/// so the title is the key. Searches the window of a known terminal process, or every Windows Terminal window.
/// </summary>
internal static class UiAutomationTabs
{
    private const string TerminalProcessName = "WindowsTerminal";

    // PublicationOnly caches no exception, so a client that failed to start is tried again on the next Enter; a race at
    // most leaves one extra client to the collector, once.
    private static readonly Lazy<Automation> Shared = new(Automation.Create, LazyThreadSafetyMode.PublicationOnly);

    public static bool Select(TerminalWindow window, string title)
    {
        try
        {
            Automation automation = Shared.Value;

            return TerminalProcesses(window).Any(processId => SelectIn(automation, processId, title));
        }
        catch (COMException)
        {
            return false;
        }
    }

    private static List<int> TerminalProcesses(TerminalWindow window) => window switch
    {
        TerminalWindow.Owning owning => [owning.ProcessId],
        TerminalWindow.Any => EveryTerminalProcess(),
        _ => throw new UnreachableException(),
    };

    private static List<int> EveryTerminalProcess()
    {
        List<int> found = [];
        foreach (Process process in Process.GetProcessesByName(TerminalProcessName))
        {
            using (process)
            {
                found.Add(process.Id);
            }
        }

        return found;
    }

    /// <summary>Searches one process's windows; every COM object the search obtains is released as it returns, and the shared ones are left alone.</summary>
    private static bool SelectIn(Automation automation, int processId, string title)
    {
        using ComObjects held = new();
        IUIAutomationCondition ownedWindow = held.Keep(automation.Client.CreatePropertyCondition(UIA_PropertyIds.UIA_ProcessIdPropertyId, processId));
        IUIAutomationElementArray windows = held.Keep(automation.Desktop.FindAll(TreeScope.TreeScope_Children, ownedWindow));
        for (int index = 0; index < windows.Length; index++)
        {
            IUIAutomationElement owned = held.Keep(windows.GetElement(index));
            IUIAutomationElementArray tabs = held.Keep(owned.FindAll(TreeScope.TreeScope_Descendants, automation.TabItem));
            if (SelectShowing(tabs, title, held))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Selects the first tab whose title shows the session's name. UI Automation matches only a whole name, and Claude
    /// Code may have put a glyph before it, so every tab is read and <see cref="TabTitle"/> decides.
    /// </summary>
    private static bool SelectShowing(IUIAutomationElementArray tabs, string title, ComObjects held)
    {
        for (int index = 0; index < tabs.Length; index++)
        {
            IUIAutomationElement tab = held.Keep(tabs.GetElement(index));
            if (TabTitle.Shows(tab.CurrentName ?? string.Empty, title)
                && held.Keep(tab.GetCurrentPattern(UIA_PatternIds.UIA_SelectionItemPatternId)) is IUIAutomationSelectionItemPattern selection)
            {
                selection.Select();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The one client the process keeps, as UI Automation intends, with the parts every search shares: the desktop and
    /// the condition for a tab item. Its objects are free-threaded, so the pool threads the searches run on share it.
    /// </summary>
    private sealed record Automation(IUIAutomation Client, IUIAutomationElement Desktop, IUIAutomationCondition TabItem)
    {
        public static Automation Create()
        {
            IUIAutomation client = new CUIAutomation8();

            return new Automation(client, client.GetRootElement(), client.CreatePropertyCondition(UIA_PropertyIds.UIA_ControlTypePropertyId, UIA_ControlTypeIds.UIA_TabItemControlTypeId));
        }
    }

    /// <summary>The COM objects one search obtained, released last first when it is disposed; each is held once, so one release is exact.</summary>
    private sealed class ComObjects : IDisposable
    {
        private readonly List<object> held = [];

        [return: NotNullIfNotNull(nameof(value))]
        public T? Keep<T>(T? value) where T : class
        {
            if (value is not null)
            {
                held.Add(value);
            }

            return value;
        }

        public void Dispose()
        {
            // Always true here: only Windows has UI Automation, and the guard is what the platform analyser reads.
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            for (int index = held.Count - 1; index >= 0; index--)
            {
                Marshal.ReleaseComObject(held[index]);
            }
        }
    }
}