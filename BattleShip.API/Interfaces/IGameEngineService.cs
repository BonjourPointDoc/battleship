using Battleship.Contracts;
using BattleShip.Models;

public interface IGameEngineService
{
    ShotResultDto ProcessShot(Board targetBoard, Position target);
    GameStateDto ToDto(Game game);
    bool IsValidPosition(Position pos);
}