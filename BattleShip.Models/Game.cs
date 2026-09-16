public sealed class Game
{
    public Guid Id { get; init; }

    public Board PlayerBoard { get; init; } = new();

    public Board AiBoard { get; init; } = new();

    public Guid CurrentPlayerId { get; set; }

    public Guid PlayerId { get; init; }

    public Guid AiId { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}