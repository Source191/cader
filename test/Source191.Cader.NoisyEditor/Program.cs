using System.Runtime.InteropServices;
using System.Text;

namespace Source191.Cader.NoisyEditor;

/// <summary>
/// A stand-in for an editor that prints as it starts, as VS Code does: a windowed (GUI subsystem) program which, when it
/// was handed no standard output, attaches to its parent's console as Electron does unless
/// <see href="https://github.com/electron/electron/blob/main/docs/api/environment-variables.md">ELECTRON_NO_ATTACH_CONSOLE</see>
/// is set, and writes a line to standard output and one to standard error. It then writes the file named by its first
/// argument: the ids of the processes on the console it attached to (none when it attached to none) on the first line,
/// and each of its other arguments on a line of its own, so a test knows both lines have been written, where they went
/// and what it was given.
/// </summary>
public static partial class Program
{
    public const string StandardOutputLine = "Northwind editor standard output";
    public const string StandardErrorLine = "Northwind editor standard error";

    private const int StandardOutputHandle = -11;
    private const uint AttachParentProcess = uint.MaxValue;

    public static void Main(string[] args)
    {
        string consoleProcesses = string.Empty;
        nint output = GetStdHandle(StandardOutputHandle);
        if ((output == nint.Zero || output == -1) && AttachConsole(AttachParentProcess))
        {
            consoleProcesses = ConsoleProcesses();
            WriteToConsole();
        }

        Console.Out.WriteLine(StandardOutputLine);
        Console.Out.Flush();
        Console.Error.WriteLine(StandardErrorLine);
        Console.Error.Flush();
        File.WriteAllLines(args[0], [consoleProcesses, .. args.Skip(1)]);
    }

    private static string ConsoleProcesses()
    {
        uint[] ids = new uint[64];
        uint count = GetConsoleProcessList(ids, (uint)ids.Length);

        return string.Join(' ', ids.Take((int)Math.Min(count, (uint)ids.Length)));
    }

    private static void WriteToConsole()
    {
        using FileStream console = new("CONOUT$", FileMode.Open, FileAccess.Write);
        console.Write(Encoding.UTF8.GetBytes(StandardOutputLine + Environment.NewLine + StandardErrorLine + Environment.NewLine));
    }

    [LibraryImport("kernel32.dll")]
    private static partial nint GetStdHandle(int standardHandle);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AttachConsole(uint processId);

    [LibraryImport("kernel32.dll")]
    private static partial uint GetConsoleProcessList([Out] uint[] processIds, uint count);
}