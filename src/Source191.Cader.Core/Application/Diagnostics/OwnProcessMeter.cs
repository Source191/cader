using Source191.Cader.Domain;

namespace Source191.Cader.Application.Diagnostics;

/// <summary>Reads Cader's own handle count and private memory from the process table, each as unknown when it is refused.</summary>
public sealed class OwnProcessMeter(IProcessRegistry processes, int processId)
{
    public OwnHolding Read() => new(HandlesNow(), MemoryNow());

    private Reading<int> HandlesNow() => processes.HandleCountOf(processId) is int handles ? new Reading<int>.Read(handles) : new Reading<int>.Unknown();

    private Reading<MemorySize> MemoryNow() => processes.UsageOf(processId) is ProcessUsage usage ? new Reading<MemorySize>.Read(usage.PrivateMemory) : new Reading<MemorySize>.Unknown();
}
