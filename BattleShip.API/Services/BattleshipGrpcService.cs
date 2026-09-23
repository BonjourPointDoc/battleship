using Battleship.Grpc;
using BattleShip.Models;
using Grpc.Core;
using System.Collections.Concurrent;

namespace Battleship.Services;

public class BattleshipGrpcService : BattleshipGrpc.BattleshipGrpcBase
{
    private static readonly ConcurrentDictionary<Guid, Game> Games = new();

    public override Task<ListGamesResponse> GetGames(Empty request, ServerCallContext context)
    {
        var response = new ListGamesResponse();
        var allGames = Games.Values
            .OrderByDescending(g => g.CreatedAt)
            .Select(MapToGameStateMessage);

        response.Games.AddRange(allGames);
        return Task.FromResult(response);
    }

    public override Task<GameStateMessage> CreateGame(Empty request, ServerCallContext context)
    {
        var playerId = Guid.NewGuid();
        var aiId = Guid.NewGuid();
        var isPlayerFirst = Random.Shared.Next(2) == 0;

        var game = new Game
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            AiId = aiId,
            CurrentPlayerId = isPlayerFirst ? playerId : aiId,
            AiBoard = GenerateRandomAiBoard(),
            PlayerBoard = new Board(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        Games[game.Id] = game;
        return Task.FromResult(MapToGameStateMessage(game));
    }

    public override Task<GameStateMessage> GetGame(GetGameRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var gameId) || !Games.TryGetValue(gameId, out var game))
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Partie introuvable."));
        }

        return Task.FromResult(MapToGameStateMessage(game));
    }

    public override Task<PlaceShipsGrpcResponse> PlaceShips(PlaceShipsGrpcRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.GameId, out var gameId) || !Games.TryGetValue(gameId, out var game))
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Partie introuvable."));
        }

        if (game.PlayerBoard.Ships.Count > 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Les bateaux sont déjà placés."));
        }

        var domainShips = request.Ships.Select(MapToDomainShip).ToList();
        game.PlayerBoard = game.PlayerBoard.WithShips(domainShips);

        ShotResultGrpc? initialAiShot = null;
        if (game.CurrentPlayerId == game.AiId)
        {
            initialAiShot = ExecuteAiTurn(game);
        }

        var response = new PlaceShipsGrpcResponse
        {
            Game = MapToGameStateMessage(game),
            InitialAiShot = initialAiShot
        };

        return Task.FromResult(response);
    }

    public override Task<TurnResponseGrpc> TakeShot(TakeShotGrpcRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.GameId, out var gameId) || !Games.TryGetValue(gameId, out var game))
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Partie introuvable."));
        }

        if (game.PlayerBoard.IsGameOver() || game.AiBoard.IsGameOver())
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "La partie est terminée."));
        }

        if (game.CurrentPlayerId != game.PlayerId)
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, "Ce n'est pas votre tour."));
        }

        var target = new Position(request.Target.Row, request.Target.Column);

        if (game.AiBoard.IsShot(target))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Cette case a déjà été ciblée."));
        }

        game.AiBoard = game.AiBoard.WithShot(target);
        var playerShotResult = ProcessShot(game.AiBoard, target);

        if (game.AiBoard.IsGameOver())
        {
            return Task.FromResult(new TurnResponseGrpc
            {
                PlayerShotResult = playerShotResult,
                Status = GameState.Finished.ToString(),
                CurrentPlayerId = game.CurrentPlayerId.ToString(),
                WinnerId = game.PlayerId.ToString()
            });
        }

        game.CurrentPlayerId = game.AiId;
        var aiShotResult = ExecuteAiTurn(game);

        var winnerId = game.PlayerBoard.IsGameOver() ? game.AiId : (Guid?)null;
        var status = winnerId.HasValue ? GameState.Finished : GameState.InProgress;

        var response = new TurnResponseGrpc
        {
            PlayerShotResult = playerShotResult,
            AiShotResult = aiShotResult,
            Status = status.ToString(),
            CurrentPlayerId = game.CurrentPlayerId.ToString()
        };

        if (winnerId.HasValue)
        {
            response.WinnerId = winnerId.Value.ToString();
        }

        return Task.FromResult(response);
    }

    public override Task<DeleteResponse> DeleteAllGames(Empty request, ServerCallContext context)
    {
        Games.Clear();
        return Task.FromResult(new DeleteResponse { Message = "Toutes les parties ont été supprimées." });
    }

    public override Task<DeleteResponse> DeleteGame(DeleteGameRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var gameId) || !Games.TryRemove(gameId, out _))
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Partie introuvable."));
        }

        return Task.FromResult(new DeleteResponse { Message = $"La partie {gameId} a été supprimée." });
    }

    #region Helpers & Mappers

    private static ShotResultGrpc ExecuteAiTurn(Game game)
    {
        Position aiTarget;
        var random = Random.Shared;

        do
        {
            aiTarget = new Position(random.Next(0, 10), random.Next(0, 10));
        } while (game.PlayerBoard.IsShot(aiTarget));

        game.PlayerBoard = game.PlayerBoard.WithShot(aiTarget);
        var aiResult = ProcessShot(game.PlayerBoard, aiTarget);

        if (!game.PlayerBoard.IsGameOver())
        {
            game.CurrentPlayerId = game.PlayerId;
        }

        return aiResult;
    }

    private static GameStateMessage MapToGameStateMessage(Game game)
    {
        var playerHits = game.PlayerBoard.Shots.Where(game.PlayerBoard.IsHit).Select(MapPosition);
        var playerMisses = game.PlayerBoard.Shots.Where(game.PlayerBoard.IsMiss).Select(MapPosition);
        var aiHits = game.AiBoard.Shots.Where(game.AiBoard.IsHit).Select(MapPosition);
        var aiMisses = game.AiBoard.Shots.Where(game.AiBoard.IsMiss).Select(MapPosition);

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

        var message = new GameStateMessage
        {
            Id = game.Id.ToString(),
            PlayerId = game.PlayerId.ToString(),
            AiId = game.AiId.ToString(),
            CurrentPlayerId = game.CurrentPlayerId.ToString(),
            Status = status.ToString(),
            CreatedAt = game.CreatedAt.ToString("o"),
            PlayerBoard = new BoardMessage(),
            AiBoard = new BoardMessage()
        };

        if (winnerId.HasValue)
        {
            message.WinnerId = winnerId.Value.ToString();
        }

        message.PlayerBoard.Shots.AddRange(game.PlayerBoard.Shots.Select(MapPosition));
        message.PlayerBoard.Hits.AddRange(playerHits);
        message.PlayerBoard.Misses.AddRange(playerMisses);
        message.PlayerBoard.Ships.AddRange(game.PlayerBoard.Ships.Select(MapShip));

        message.AiBoard.Shots.AddRange(game.AiBoard.Shots.Select(MapPosition));
        message.AiBoard.Hits.AddRange(aiHits);
        message.AiBoard.Misses.AddRange(aiMisses);

        return message;
    }

    private static ShotResultGrpc ProcessShot(Board targetBoard, Position target)
    {
        var isHit = targetBoard.IsHit(target);
        var hitShip = isHit ? targetBoard.Ships.FirstOrDefault(s => s.GetPositions().Contains(target)) : null;
        var isSunk = hitShip != null && targetBoard.IsSunk(hitShip);

        var result = new ShotResultGrpc
        {
            Target = new PositionMessage { Row = target.Row, Column = target.Column },
            IsHit = isHit,
            IsSunk = isSunk
        };

        if (isSunk && hitShip != null)
        {
            result.SunkShipType = hitShip.Type.ToString();
        }

        return result;
    }

    private static PositionMessage MapPosition(Position pos) => new() { Row = pos.Row, Column = pos.Column };

    private static ShipMessage MapShip(Ship ship) => new()
    {
        Type = ship.Type.ToString(),
        Direction = ship.Direction.ToString(),
        BowPosition = MapPosition(ship.Position)
    };

    private static Ship MapToDomainShip(ShipMessage msg) => new(
        Enum.Parse<ShipType>(msg.Type),
        new Position(msg.BowPosition.Row, msg.BowPosition.Column),
        Enum.Parse<Direction>(msg.Direction)
    );

    private static Board GenerateRandomAiBoard()
    {
        var ships = new List<Ship>();
        var shipTypes = Enum.GetValues<ShipType>();
        var random = Random.Shared;

        foreach (var type in shipTypes)
        {
            bool placed = false;
            while (!placed)
            {
                var dir = (Direction)random.Next(2);
                var row = random.Next(0, 10);
                var col = random.Next(0, 10);

                var candidate = new Ship(type, new Position(row, col), dir);
                var positions = candidate.GetPositions().ToList();

                if (positions.Any(p => p.Row is < 0 or >= 10 || p.Column is < 0 or >= 10))
                    continue;

                if (ships.SelectMany(s => s.GetPositions()).Any(p => positions.Contains(p)))
                    continue;

                ships.Add(candidate);
                placed = true;
            }
        }

        return new Board { Ships = ships };
    }

    #endregion
}