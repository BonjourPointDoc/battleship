using BattleShip.Models;
namespace Battleship.Contracts;

public record PlaceShipsRequest(List<Ship> Ships);