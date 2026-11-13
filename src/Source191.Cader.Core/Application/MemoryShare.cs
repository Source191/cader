using Source191.Cader.Domain;

namespace Source191.Cader.Application;

/// <summary>One program a session started, with everything under it: an MCP server, a shell, whatever else.</summary>
public sealed record MemoryShare(string Label, MemorySize Memory);