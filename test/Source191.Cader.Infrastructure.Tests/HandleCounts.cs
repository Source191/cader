using System.Diagnostics;

namespace Source191.Cader.Infrastructure.Tests;

/// <summary>
/// This process's handle count once a full collection has run, so a test can tell handles code left open from those the
/// collector would reclaim. A test that counts joins the "Machine-wide pings" collection, so no other test opens handles meanwhile.
/// </summary>
internal static class HandleCounts
{
    public static int Settled()
    {
#pragma warning disable S1215 // The count must not include handles a collection would reclaim.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
#pragma warning restore S1215
        using Process current = Process.GetCurrentProcess();

        return current.HandleCount;
    }
}