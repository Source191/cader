using Source191.Cader.Application.Views;

namespace Source191.Cader.Tui;

/// <summary>The route each kind of effect takes from a tab, by its marker; an unmarked effect goes to its handler.</summary>
internal static class EffectRoutes
{
    public static EffectRoute Of(Type effect)
    {
        return effect switch
        {
            _ when effect.IsAssignableTo(typeof(ILeavesScreen)) => EffectRoute.Leave,
            _ when effect.IsAssignableTo(typeof(ISwitchesTab)) => EffectRoute.Switch,
            _ when effect.IsAssignableTo(typeof(IConsultsSessions)) => EffectRoute.Consult,
            _ when effect.IsAssignableTo(typeof(IDialogEffect)) => EffectRoute.Dialog,
            _ when effect.IsAssignableTo(typeof(IRefreshesTab)) => EffectRoute.Refresh,
            _ => EffectRoute.Dispatch,
        };
    }
}