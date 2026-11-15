namespace Source191.Cader.Application.Diagnostics;

/// <summary>The diagnostic log's files were cleared: how many were deleted and their bytes, and how many were kept because another program had one open. Numbers only, never a path.</summary>
public sealed record DiagnosticLogCleared(int FilesRemoved, long BytesRemoved, int FilesKept) : DiagnosticEvent;