namespace Source191.Cader.Application.Diagnostics;

/// <summary>
/// One thing Cader records about its own behaviour. The set is closed and every member is a number, a duration, a
/// version, an enum or a value built only from Cader's code (<see cref="CodeName"/>, <see cref="ExceptionSummary"/>,
/// <see cref="ChildCommand"/>); <c>DiagnosticEventsShould</c> fails when a case could hold anything else.
/// </summary>
public abstract record DiagnosticEvent;