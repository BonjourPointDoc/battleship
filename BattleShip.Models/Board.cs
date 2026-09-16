namespace BattleShip.Models;

public sealed class Board
{
    public IReadOnlyList<Ship> Ships { get; init; } = [];

    public IReadOnlyList<Position> Shots { get; init; } = [];

    bool IsShot(Position position){
        return Shots.Contains(position);
    }

    bool HasShip(Position position){
        return Ships.Any(ship => ship.GetPositions().Contains(position));
    }

    bool IsHit(Position position){
        return Ships.Any(ship =>
            ship.GetPositions().Contains(position))
            && Shots.Contains(position);
    }

    bool IsMiss(Position position){
        return Shots.Contains(position)
            && !HasShip(position);
    }

    bool IsSunk(Ship ship){
        return ship.GetPositions()
            .All(position => Shots.Contains(position));
    }

    bool IsGameOver(){
        return Ships.All(IsSunk);
    }
}