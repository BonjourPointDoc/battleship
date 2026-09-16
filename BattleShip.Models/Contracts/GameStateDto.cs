namespace Battleship.Contracts;

public record GameStateDto(
    Guid Id,
    Guid PlayerId,
    Guid AiId,
    Guid CurrentPlayerId,
    GameState Status,
    Guid? WinnerId,
    BoardDto PlayerBoard,
    BoardDto AiBoard
);