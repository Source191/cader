namespace Source191.Cader.Ports;

public sealed record ProcessRequest(string FileName, IReadOnlyList<string> Arguments, WorkingFolder WorkingDirectory);