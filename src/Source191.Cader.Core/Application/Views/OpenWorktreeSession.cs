using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>Enter on a worktree: open its session's tab as the Sessions tab's Enter does; by id, because names are not unique, and the name for when it is not listed.</summary>
public sealed record OpenWorktreeSession(SessionId SessionId, ListedName Name) : ViewEffect, IConsultsSessions;