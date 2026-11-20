using Source191.Cader.Domain;

namespace Source191.Cader.Application.Launch;

/// <summary>The step a launch has begun: which launch, which stage, and its number out of the steps that launch runs; <see cref="None"/> before any launch has begun one.</summary>
public sealed record LaunchStep(SessionName Name, LaunchStage Stage, int Number, int Count, LaunchBase Base)
{
    /// <summary>No step yet: numbered 0, so it decorates no launch's words.</summary>
    public static LaunchStep None { get; } = new(SessionName.From("none"), LaunchStage.Fetching, 0, 0, new LaunchBase.None());
}