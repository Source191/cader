using Source191.Cader.Application.Launch;

namespace Source191.Cader.Application.Views;

/// <summary>Start the session the launch wizard planned, from any tab, and open its tab once the supervisor lists it.</summary>
public sealed record StartSession(LaunchPlan Plan) : ViewEffect;