using Battleship.Contracts;
using BattleShip.Models;

public class GameEngineService : IGameEngineService
{
    public ShotResultDto ProcessShot(Board targetBoard, Position target)
    {
        var isHit = targetBoard.IsHit(target);
        var hitShip = isHit ? targetBoard.Ships.FirstOrDefault(s => s.GetPositions().Contains(target)) : null;
        var isSunk = hitShip != null && targetBoard.IsSunk(hitShip);

        return new ShotResultDto(
            target,
            isHit,
            isSunk,
            isSunk ? hitShip?.Type : null
        );
    }

    public bool IsValidPosition(Position pos)
    {
        return pos.Row is >= 0 and < 10 && pos.Column is >= 0 and < 10;
    }

    public GameStateDto ToDto(Game game)
    {
        var playerHits = game.PlayerBoard.Shots.Where(game.PlayerBoard.IsHit).ToList();
        var playerMisses = game.PlayerBoard.Shots.Where(game.PlayerBoard.IsMiss).ToList();

        var aiHits = game.AiBoard.Shots.Where(game.AiBoard.IsHit).ToList();
        var aiMisses = game.AiBoard.Shots.Where(game.AiBoard.IsMiss).ToList();

        Guid? winnerId = null;
        GameState status = GameState.InProgress;

        if (game.PlayerBoard.Ships.Count == 0)
        {
            status = GameState.WaitingForPlayerBoard;
        }
        else if (game.AiBoard.IsGameOver())
        {
            status = GameState.Finished;
            winnerId = game.PlayerId;
        }
        else if (game.PlayerBoard.IsGameOver())
        {
            status = GameState.Finished;
            winnerId = game.AiId;
        }

        return new GameStateDto(
            game.Id,
            game.PlayerId,
            game.AiId,
            game.CurrentPlayerId,
            status,
            winnerId,
            new BoardDto(game.PlayerBoard.Shots, playerHits, playerMisses, game.PlayerBoard.Ships),
            new BoardDto(game.AiBoard.Shots, aiHits, aiMisses, null),
            game.CreatedAt
        );
    }
}