using System;
using System.Linq;
using Battleship.Grpc;
using BattleShip.Models;

namespace BattleShip.App.Services;

public static class GrpcMappingExtensions
{
    public static Position ToModel(this PositionMessage msg) => new(msg.Row, msg.Column);

    public static PositionMessage ToProto(this Position pos) => new()
    {
        Row = pos.Row,
        Column = pos.Column
    };

    public static Ship ToModel(this ShipMessage msg)
    {
        Enum.TryParse<ShipType>(msg.Type, true, out var type);
        Enum.TryParse<Direction>(msg.Direction, true, out var dir);
        return new Ship(type, msg.BowPosition.ToModel(), dir);
    }

    public static ShipMessage ToProto(this Ship ship) => new()
    {
        Type = ship.Type.ToString(),
        BowPosition = ship.Position.ToProto(),
        Direction = ship.Direction.ToString()
    };

    public static BoardDto ToModel(this BoardMessage msg) => new()
    {
        Shots = msg.Shots.Select(s => s.ToModel()).ToList(),
        Hits = msg.Hits.Select(h => h.ToModel()).ToList(),
        Misses = msg.Misses.Select(m => m.ToModel()).ToList(),
        Ships = msg.Ships.Select(s => s.ToModel()).ToList()
    };

    public static GameStateDto ToModel(this GameStateMessage msg) => new()
    {
        Id = Guid.Parse(msg.Id),
        PlayerId = Guid.Parse(msg.PlayerId),
        AiId = Guid.Parse(msg.AiId),
        CurrentPlayerId = Guid.Parse(msg.CurrentPlayerId),
        Status = int.TryParse(msg.Status, out var s) ? s : (msg.Status == "Finished" ? 2 : msg.Status == "InProgress" ? 1 : 0),
        WinnerId = string.IsNullOrEmpty(msg.WinnerId) ? null : Guid.Parse(msg.WinnerId),
        PlayerBoard = msg.PlayerBoard?.ToModel() ?? new(),
        AiBoard = msg.AiBoard?.ToModel() ?? new(),
        CreatedAt = DateTimeOffset.TryParse(msg.CreatedAt, out var dt) ? dt : DateTimeOffset.UtcNow
    };

    public static ShotResultDto ToModel(this ShotResultGrpc msg) => new()
    {
        Position = msg.Target.ToModel(),
        IsHit = msg.IsHit,
        IsSunk = msg.IsSunk,
        SunkShipType = Enum.TryParse<ShipType>(msg.SunkShipType, true, out var type) ? type : null
    };
}