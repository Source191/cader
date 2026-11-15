using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;

namespace Source191.Cader.ParentStandIn;

/// <summary>
/// A stand-in for the console as the parent of a captured child, which a test kills as a crash or a closed tab would end
/// Cader, and for that child as it starts a program of its own. Its arguments are how to start (<c>kill-on-parent-exit</c>,
/// <c>plain</c> or <c>shell-execute</c>), what to do then (<c>wait</c> to be killed, or <c>exit</c>), the file to write the
/// started program's id to, and the program with its arguments.
/// </summary>
[SupportedOSPlatform("windows")]
public static class Program
{
    public static void Main(string[] args)
    {
        ProcessStartInfo start = new(args[3], args.Skip(4))
        {
            UseShellExecute = args[0] == "shell-execute",
            CreateNoWindow = args[0] != "shell-execute",
            WindowStyle = ProcessWindowStyle.Hidden,
            KillOnParentExit = args[0] == "kill-on-parent-exit",
        };
        using Process child = Process.Start(start) ?? throw new InvalidOperationException("The program did not start.");

        // Written beside and moved into place, so a reader never sees half of it.
        File.WriteAllText(args[2] + ".tmp", child.Id.ToString(CultureInfo.InvariantCulture));
        File.Move(args[2] + ".tmp", args[2]);
        if (args[1] == "wait")
        {
            Thread.Sleep(Timeout.Infinite);
        }
    }
}