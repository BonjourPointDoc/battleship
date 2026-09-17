namespace Battleship.Contracts;
using BattleShip.Models;

public record TurnResponseDto(
    ShotResultDto PlayerShotResult,
    ShotResultDto? AiShotResult,
    GameState GameStatus,
    Guid CurrentPlayerId,
    Guid? WinnerId
);