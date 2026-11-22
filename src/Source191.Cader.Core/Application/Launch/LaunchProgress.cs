using Source191.Cader.Application.Views;

namespace Source191.Cader.Application.Launch;

/// <summary>
/// The step the latest console launch has begun: the launch service and the start handler report into it from a pool
/// thread and only replace the step, and the screen's ticker asks it for the status line's words on the UI thread. It calls
/// no one, so a launch never waits on the UI thread, which a dialog opened inside <c>app.Invoke</c> would hold.
/// </summary>
public sealed class LaunchProgress : IProgress<LaunchStep>
{
    private readonly SharedSnapshot<LaunchStep> latest = new(LaunchStep.None);

    public LaunchStep Latest => latest.Value;

    public void Report(LaunchStep value) => latest.Replace(value);

    /// <summary>The launch's step in place of its plain working words, only for the launch the step belongs to; any other message as it is.</summary>
    public StatusMessage Shown(StatusMessage message) =>
        message is StatusMessage.Working { Effect: StartSession start } && Latest is { Number: > 0 } step && step.Name == start.Plan.Name
            ? new StatusMessage.Launching(step)
            : message;
}