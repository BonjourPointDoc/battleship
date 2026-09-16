namespace Battleship.Contracts;

public record TurnResponseDto(
    ShotResultDto PlayerShotResult,
    ShotResultDto? AiShotResult,
    GameState GameStatus,
    Guid CurrentPlayerId,
    Guid? WinnerId
);