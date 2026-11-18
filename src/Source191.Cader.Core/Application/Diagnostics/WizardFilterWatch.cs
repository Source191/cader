using Source191.Cader.Application.Views;
using Source191.Cader.Domain;

namespace Source191.Cader.Application.Diagnostics;

/// <summary>
/// One wizard's diagnostic facts: the repositories it offered as it opened, then its first filter change, once. A wizard
/// on a fixed folder has no filter, so it records nothing. The filter's characters are never recorded, only the kind of
/// the first and how many there are. Used on the UI thread only.
/// </summary>
public sealed class WizardFilterWatch
{
    private readonly DiagnosticLog log;
    private readonly TimeProvider clock;
    private readonly long opened;
    private bool listFilled;
    private bool watching;

    private WizardFilterWatch(DiagnosticLog log, TimeProvider clock, bool watching)
    {
        this.log = log;
        this.clock = clock;
        this.watching = watching;
        opened = clock.GetTimestamp();
    }

    public static WizardFilterWatch Open(WizardState state, DiagnosticLog log, TimeProvider clock)
    {
        if (state.ShowsChooser)
        {
            log.Record(new WizardOpened(state.AllChoices.RepositoryCount));
        }

        return new WizardFilterWatch(log, clock, watching: state.ShowsChooser);
    }

    /// <summary>The dialog has put the choices in its list.</summary>
    public void ListFilled() => listFilled = true;

    /// <summary>The filter now holds this text: the first time it holds anything, the change is recorded.</summary>
    public void FilterChanged(string text)
    {
        if (!watching || text.Length == 0)
        {
            return;
        }

        watching = false;
        log.Record(new WizardFilterStarted(clock.GetElapsedTime(opened), CharacterKinds.First(text), text.EnumerateRunes().Count(), listFilled));
    }
}