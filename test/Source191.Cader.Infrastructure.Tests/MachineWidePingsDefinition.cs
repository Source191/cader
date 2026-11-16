namespace Source191.Cader.Infrastructure.Tests;

/// <summary>
/// A test class that checks no <c>ping</c> it started outlives it looks at every ping on the machine, so it joins this
/// collection, which runs alone: no other class can start a ping while it looks. A test that counts this process's
/// handles around many runs joins it for the same reason: no other test opens handles while it counts.
/// </summary>
[CollectionDefinition("Machine-wide pings", DisableParallelization = true)]
public sealed class MachineWidePingsDefinition;