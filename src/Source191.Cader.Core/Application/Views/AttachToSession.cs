using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>
/// Attach to a session in this terminal: an <see cref="ILeavesScreen"/> only <c>cader here</c> raises;
/// <see cref="ScreenLoop"/> refuses it from the console, which never hands its terminal to a child.
/// </summary>
public sealed record AttachToSession(JobId JobId) : ViewEffect, ILeavesScreen;