using Source191.Vantage.Ports;

namespace Source191.Vantage.ReadmeScene;

/// <summary>Plays a key script into the scene's console: keys through <see cref="ConsoleKeys"/>, focus through <c>wt focus-tab</c>, marks as files.</summary>
internal sealed class KeyPlayer(SceneFolders folders, IProcessRunner processes, string wt)
{
    public async Task PlayAsync(IReadOnlyList<KeyStep> steps, CancellationToken cancellationToken)
    {
        foreach (KeyStep step in steps)
        {
            await Task.Delay(step.Delay, cancellationToken);
            await TakeAsync(step, cancellationToken);
        }
    }

    private async Task TakeAsync(KeyStep step, CancellationToken cancellationToken)
    {
        switch (step)
        {
            case KeyStep.Type type:
                foreach (char character in type.Text)
                {
                    ConsoleKeys.Type(character);
                    await Task.Delay(KeyScript.TypingInterval, cancellationToken);
                }

                break;
            case KeyStep.Press press:
                ConsoleKeys.Press(press.Key);
                break;
            case KeyStep.FocusVantage:
                // Vantage's own tab is the scene window's first, and the window is the one used last.
                await processes.RunAsync(new ProcessRequest(wt, ["-w", "0", "focus-tab", "--target", "0"], WorkingFolder.Inherit), cancellationToken);
                break;
            case KeyStep.Mark mark:
                await File.WriteAllTextAsync(Path.Combine(folders.Root, $"{mark.Name}.mark"), string.Empty, cancellationToken);
                break;
            default:
                throw new InvalidOperationException($"A key step the player does not know: {step}");
        }
    }
}
