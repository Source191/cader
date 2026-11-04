namespace Source191.Cader.Ports;

public sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);