using Source191.Cader.Application.Views;

namespace Source191.Cader.Application.Launch;

/// <summary>
/// The step the latest console launch has begun: the launch service and the start handler report into it from a pool
/// thread, and the screen asks it for the status line's words on the UI thread. It calls one listener, registered once
/// by the screen, after each step, so the screen can redraw; a no-op until then, so <c>cader here</c>, which reports into
/// it with no screen, costs nothing.
/// </summary>
public sealed class LaunchProgress : IProgress<LaunchStep>
{
    private readonly SharedSnapshot<LaunchStep> latest = new(LaunchStep.None);
    private readonly SharedSnapshot<Action> listener = new(() => { });

    public LaunchStep Latest => latest.Value;

    /// <summary>Called after each step from the thread that reports it.</summary>
    public void Follow(Action onStep) => listener.Replace(onStep);

    public void Report(LaunchStep value)
    {
        latest.Replace(value);
        listener.Value();
    }

    /// <summary>The launch's step in place of its plain working words, only for the launch the step belongs to; any other message as it is.</summary>
    public StatusMessage Shown(StatusMessage message) =>
        message is StatusMessage.Working { Effect: StartSession start } && Latest is { Number: > 0 } step && step.Name == start.Plan.Name
            ? new StatusMessage.Launching(step)
            : message;
}