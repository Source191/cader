using System.Collections.ObjectModel;

namespace Source191.Cader.Ports;

public sealed record ProcessRequest(string FileName, IReadOnlyList<string> Arguments, WorkingFolder WorkingDirectory)
{
    /// <summary>Environment variables set for this child alone, after the ones it must not inherit are dropped; none unless a caller sets them.</summary>
    public IReadOnlyDictionary<string, string> Variables { get; init; } = ReadOnlyDictionary<string, string>.Empty;
}
