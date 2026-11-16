using Source191.Cader.Application.Effects;

namespace Source191.Cader.Application.Actions;

/// <summary>What clearing the diagnostic log did, so a clear that deleted nothing never reads as if it had.</summary>
public abstract record ClearDiagnosticLogOutcome : Outcome
{
    /// <summary>Every log file there was deleted.</summary>
    public sealed record Cleared(int Files, long Bytes) : ClearDiagnosticLogOutcome;

    /// <summary>Neither file was there.</summary>
    public sealed record NothingToClear : ClearDiagnosticLogOutcome;

    /// <summary>Some files were deleted and <paramref name="Kept"/> were kept because another program had them open.</summary>
    public sealed record PartlyCleared(int Files, long Bytes, int Kept) : ClearDiagnosticLogOutcome;

    /// <summary>Every file there was kept because another program had it open.</summary>
    public sealed record NoneCleared(int Kept) : ClearDiagnosticLogOutcome;
}