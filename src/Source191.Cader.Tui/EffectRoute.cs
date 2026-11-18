namespace Source191.Cader.Tui;

/// <summary>Where a tab sends an effect its reducer raised, chosen by the effect's marker.</summary>
internal enum EffectRoute
{
    /// <summary>To the host, which ends the run (<see cref="Application.Views.ILeavesScreen"/>).</summary>
    Leave,

    /// <summary>To the host, which shows another tab (<see cref="Application.Views.ISwitchesTab"/>).</summary>
    Switch,

    /// <summary>To the host, which answers from the Sessions tab's rows (<see cref="Application.Views.IConsultsSessions"/>).</summary>
    Consult,

    /// <summary>To <see cref="DialogEffects"/> (<see cref="Application.Views.IDialogEffect"/>).</summary>
    Dialog,

    /// <summary>To the host, which refreshes the shown tab now (<see cref="Application.Views.IRefreshesTab"/>).</summary>
    Refresh,

    /// <summary>To its handler through the effect dispatcher.</summary>
    Dispatch,
}
