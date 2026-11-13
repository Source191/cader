namespace Source191.Cader.Application.Views;

/// <summary>Show this tab: a click on its title, and Esc back to Sessions.</summary>
public sealed record SelectTab(ConsoleTab Tab) : ViewEffect, ISwitchesTab;