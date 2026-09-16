using BattleShip.Models;
namespace Battleship.Contracts;


public record BoardDto(
    IReadOnlyList<Position> Shots,
    IReadOnlyList<Position> Hits,
    IReadOnlyList<Position> Misses,
    IReadOnlyList<Ship>? Ships
);