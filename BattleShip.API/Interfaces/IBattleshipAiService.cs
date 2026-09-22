using Battleship.Contracts;
using BattleShip.Models;

public interface IBattleshipAiService
{
    Board GenerateRandomBoard();
    ShotResultDto ExecuteAiTurn(Game game);
    Position SelectAiTarget(Board board);
}
