namespace BattleShip.Models;

public sealed class Game
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Board PlayerBoard { get; set; } = new();
    public Board AiBoard { get; set; } = new();
    public Guid CurrentPlayerId { get; set; }

    public Guid PlayerId { get; init; }

    public Guid AiId { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}