using Source191.Cader.Adapters.Diagnostics;
using Source191.Cader.Adapters.Files;
using Source191.Cader.Adapters.Processes;
using Source191.Cader.Adapters.State;
using Source191.Cader.Adapters.Terminal;
using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Domain;
using System.Reflection;

namespace Source191.Cader.Composition;

/// <summary>What more than one command uses: the state store, the processes, the versions, the log and the crash recorder.</summary>
internal sealed record SharedParts(
    JsonFileStore Store,
    ProcessRegistry ProcessRegistry,
    ToolVersion Version,

    string ConsoleExecutable,
    string OwnFolder,
    TerminalFragment TerminalFragment,
    TerminalSettings TerminalSettings,
    ProgramLocator Programs,
    DiagnosticLogSwitch LogSwitch,
    string LogFile,
    DiagnosticLog Log,
    PerformanceCounts Counts,
    RecordedProcessRunner Processes,
    CrashFile CrashFile,
    CrashRecorder Crashes)
{
    public static SharedParts ForCurrentUser()
    {
        string stateFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Cader");
        JsonFileStore store = new(stateFolder, TimeProvider.System);
        ProcessRegistry processRegistry = new();
        ToolVersion version = ToolVersion.FromInformational(Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty);

        string consoleExecutable = Environment.ProcessPath ?? "cader";
        string ownFolder = ProgramFolder.Of(AppContext.BaseDirectory, Environment.ProcessPath);
        TerminalFragment terminalFragment = TerminalFragment.ForCurrentUser(Path.Combine(ownFolder, "logo.ico"));
        TerminalSettings terminalSettings = TerminalSettings.ForCurrentUser();

        // Every program the console starts is found on the PATH once and run by its full path, never by a bare name that Windows
        // would look for in the folder the console was started from first
        // (https://learn.microsoft.com/windows/win32/api/processthreadsapi/nf-processthreadsapi-createprocessw).
        ProgramLocator programs = ProgramLocator.ForCurrentUser();

        // The diagnostic log is always kept in memory. Its file sink is always present and written only while the Diagnostic log setting is on; the switch
        // starts off, so nothing is written until a command has read config.json and pointed it at the setting.
        string logFile = Path.Combine(stateFolder, "cader.log");
        DiagnosticLogSwitch logSwitch = new();
        DiagnosticLog log = new(TimeProvider.System, [new DiagnosticFile(logFile, logSwitch.IsOn)]);
        ProcessCounts processCounts = new();
        PerformanceCounts counts = new(processCounts, new RefreshCounts(TimeProvider.System, log), new StallCounts());
        // How long a captured run may take: a claude agents listing takes about two seconds, the slowest gh or az call a few.
        TimeSpan captureLimit = TimeSpan.FromSeconds(60);
        RecordedProcessRunner processes = new(new ProcessRunner(captureLimit), log, processCounts, TimeProvider.System);
        CrashFile crashFile = new(stateFolder);
        CrashRecorder crashes = new(log, counts, crashFile, TimeProvider.System);

        return new SharedParts(store, processRegistry, version, consoleExecutable, ownFolder, terminalFragment, terminalSettings, programs, logSwitch, logFile, log, counts, processes, crashFile, crashes);
    }

    // The start facts: versions and the terminal's size, never a path or a name.
    public Started RecordStart(ClaudeVersionReport claudeVersionReport)
    {
        Started start = Started.From(Version, claudeVersionReport, Environment.OSVersion.Version, Environment.Version, Console.WindowWidth, Console.WindowHeight);
        Log.Record(start);

        return start;
    }
}