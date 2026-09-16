using BattleShip.Models;
namespace Battleship.Contracts;

public record ShotResultDto(
    Position Position,
    bool IsHit,
    bool IsSunk,
    ShipType? SunkShipType
);