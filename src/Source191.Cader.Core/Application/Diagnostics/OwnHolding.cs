using Source191.Cader.Domain;

namespace Source191.Cader.Application.Diagnostics;

/// <summary>What Cader holds at one moment: its handle count and its private memory.</summary>
public sealed record OwnHolding(Reading<int> Handles, Reading<MemorySize> Memory);
