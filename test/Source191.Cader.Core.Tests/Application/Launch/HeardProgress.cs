using Source191.Cader.Application.Launch;

namespace Source191.Cader.Core.Tests.Application.Launch;

/// <summary>A real <see cref="LaunchProgress"/> that also tells the test each step as it is reported, on the reporting thread.</summary>
internal sealed class HeardProgress : IProgress<LaunchStep>
{
    private readonly LaunchProgress progress = new();

    public Action<LaunchStep> OnStep { get; set; } = _ => { };

    public LaunchStep Latest => progress.Latest;

    public void Report(LaunchStep value)
    {
        progress.Report(value);
        OnStep(value);
    }
}