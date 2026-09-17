namespace Battleship.Contracts;
using BattleShip.Models;

public record GameStateDto(
    Guid Id,
    Guid PlayerId,
    Guid AiId,
    Guid CurrentPlayerId,
    GameState Status,
    Guid? WinnerId,
    BoardDto PlayerBoard,
    BoardDto AiBoard,
    DateTimeOffset CreatedAt
);