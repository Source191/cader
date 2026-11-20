namespace Source191.Cader.TestDoubles;

/// <summary>
/// The bytes a synchronous call allocates on the current thread, so an allocation-budget fact is not disturbed by tests
/// running in parallel. The call runs once first to warm static initialisers, tiered compilation and a pool's first rent.
/// </summary>
public static class Allocations
{
    public static long Of(Action action)
    {
        action();
        long before = GC.GetAllocatedBytesForCurrentThread();
        action();

        return GC.GetAllocatedBytesForCurrentThread() - before;
    }
}