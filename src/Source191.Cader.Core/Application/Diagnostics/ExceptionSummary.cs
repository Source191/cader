using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Source191.Cader.Application.Diagnostics;

/// <summary>
/// What a failure looked like without anything it said: the type of the exception and of up to three inner exceptions,
/// Windows' error number for a Windows error, each with up to twenty stack frames as method names and the source file's name and line. The message, the data and
/// the folders of source paths are never read, because they can hold paths, names and program output.
/// </summary>
public sealed record ExceptionSummary
{
    private const int MostFrames = 20;
    private const int DeepestInner = 3;
    // Stryker disable once String : Accepted: a static initialiser runs once per test process and Stryker's MTP runner switches mutants without restarting it (stryker-mutator/stryker-net#3742), so the mutant cannot be attributed; ExceptionSummaryShould.LeaveOutTheFramesOfAwaitMachinery pins both namespaces
    private static readonly string[] AwaitMachinery = ["System.Runtime.CompilerServices", "System.Runtime.ExceptionServices"];

    private readonly string text;

    private ExceptionSummary(string text) => this.text = text;

    public static ExceptionSummary From(Exception exception)
    {
        List<string> parts = [];
        for (Exception? current = exception; current is not null && parts.Count <= DeepestInner; current = current.InnerException)
        {
            parts.Add(Part(current));
        }

        return new ExceptionSummary(string.Join("; inner ", parts));
    }

    public override string ToString() => text;

    private static string Part(Exception exception)
    {
        string type = Type(exception);
        List<string> frames = [.. Frames(exception).Take(MostFrames)];

        return frames.Count == 0 ? type : $"{type} at {string.Join(" < ", frames)}";
    }

    /// <summary>The exception's type, with Windows' error number for a Windows error: a number, never Windows' sentence for it.</summary>
    private static string Type(Exception exception)
    {
        string type = CodeName.For(exception.GetType()).ToString();

        return exception is Win32Exception windows ? $"{type} (error {windows.NativeErrorCode.ToString(CultureInfo.InvariantCulture)})" : type;
    }

    /// <summary>A frame without metadata (trimmed or native code) has no method, and is skipped.</summary>
    private static IEnumerable<string> Frames(Exception exception)
    {
        foreach (StackFrame frame in new StackTrace(exception, fNeedFileInfo: true).GetFrames())
        {
            if (frame.GetMethod() is MethodBase method && !IsAwaitMachinery(method))
            {
                yield return Frame(frame, method);
            }
        }
    }

    private static bool IsAwaitMachinery(MethodBase method) =>
        method.DeclaringType?.Namespace is string space && AwaitMachinery.Any(machinery => space.StartsWith(machinery, StringComparison.Ordinal));

    private static string Frame(StackFrame frame, MethodBase called)
    {
        string method = CodeName.For(called).ToString();
        string? file = frame.GetFileName();
        int line = frame.GetFileLineNumber();

        return (file, line) switch
        {
            (null, _) => method,
            // Stryker disable once String : Accepted: no managed code makes the runtime report a file with line 0 (hidden lines lose the file too), so only the branch beside it, with a line, is pinned
            (_, 0) => $"{method} ({Path.GetFileName(file)})",
            _ => $"{method} ({Path.GetFileName(file)}:{line})",
        };
    }
}