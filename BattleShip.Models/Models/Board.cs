namespace BattleShip.Models;

public sealed class Board
{
    public IReadOnlyList<Ship> Ships { get; init; } = [];
    public IReadOnlyList<Position> Shots { get; init; } = [];

    public bool IsShot(Position position) => Shots.Contains(position);

    public bool HasShip(Position position) => 
        Ships.Any(ship => ship.GetPositions().Contains(position));

    public bool IsHit(Position position) => 
        HasShip(position) && Shots.Contains(position);

    public bool IsMiss(Position position) => 
        IsShot(position) && !HasShip(position);

    public bool IsSunk(Ship ship) => 
        ship.GetPositions().All(Shots.Contains);

    public bool IsGameOver() => 
        Ships.Count > 0 && Ships.All(IsSunk);
        
    public Board WithShips(IEnumerable<Ship> newShips) => new()
    {
        Ships = newShips.ToList(),
        Shots = this.Shots
    };

    public Board WithShot(Position shot) => new()
    {
        Ships = this.Ships,
        Shots = [.. this.Shots, shot]
    };

    public IReadOnlyList<Position> GetUnsunkHits() =>
    Ships
        .Where(ship => !IsSunk(ship))
        .SelectMany(ship => ship.GetPositions())
        .Where(IsShot)
        .ToList();
}