using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

public sealed record PruneRepository(FolderPath Repository) : ViewEffect;