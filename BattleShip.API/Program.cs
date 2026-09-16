using Battleship.Contracts;
using BattleShip.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors("DevCorsPolicy");

// Stockage en mémoire des parties
var games = new ConcurrentDictionary<Guid, Game>();
var api = app.MapGroup("/api/games");

api.MapPost("/", () =>
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

    games[game.Id] = game;

    return Results.Created($"/api/games/{game.Id}", ToDto(game));
});

api.MapGet("/{id:guid}", (Guid id) =>
{
    return games.TryGetValue(id, out var game)
        ? Results.Ok(ToDto(game))
        : Results.NotFound("Partie introuvable.");
});

api.MapPost("/{id:guid}/board", (Guid id, [FromBody] PlaceShipsRequest request) =>
{
    if (!games.TryGetValue(id, out var game))
        return Results.NotFound("Partie introuvable.");

    if (game.PlayerBoard.Ships.Count > 0)
        return Results.BadRequest("Les bateaux sont déjà placés.");

    game.PlayerBoard = game.PlayerBoard.WithShips(request.Ships);

    ShotResultDto? initialAiShot = null;
    if (game.CurrentPlayerId == game.AiId)
    {
        initialAiShot = ExecuteAiTurn(game);
    }

    return Results.Ok(new
    {
        Game = ToDto(game),
        InitialAiShot = initialAiShot
    });
});

api.MapPost("/{id:guid}/shots", (Guid id, [FromBody] TakeShotRequest request) =>
{
    if (!games.TryGetValue(id, out var game))
        return Results.NotFound("Partie introuvable.");

    // Utilisation de IsGameOver()
    if (game.PlayerBoard.IsGameOver() || game.AiBoard.IsGameOver())
        return Results.BadRequest("La partie est terminée.");

    if (game.CurrentPlayerId != game.PlayerId)
        return Results.BadRequest("Ce n'est pas votre tour.");

    if (game.AiBoard.IsShot(request.Target))
        return Results.BadRequest("Cette case a déjà été ciblée.");

    game.AiBoard = game.AiBoard.WithShot(request.Target);
    var playerShotResult = ProcessShot(game.AiBoard, request.Target);

    if (game.AiBoard.IsGameOver())
    {
        return Results.Ok(new TurnResponseDto(
            playerShotResult,
            null,
            GameState.Finished,
            game.CurrentPlayerId,
            game.PlayerId
        ));
    }

    game.CurrentPlayerId = game.AiId;

    var aiShotResult = ExecuteAiTurn(game);

    var winnerId = game.PlayerBoard.IsGameOver() ? game.AiId : (Guid?)null;
    var status = winnerId.HasValue ? GameState.Finished : GameState.InProgress;

    return Results.Ok(new TurnResponseDto(
        playerShotResult,
        aiShotResult,
        status,
        game.CurrentPlayerId,
        winnerId
    ));
});

api.MapDelete("/", () =>
{
    games.Clear();
    return Results.Ok(new { Message = "Toutes les parties ont été supprimées." });
});

api.MapDelete("/{id:guid}", (Guid id) =>
{
    return games.TryRemove(id, out _)
        ? Results.Ok(new { Message = $"La partie {id} a été supprimée." })
        : Results.NotFound("Partie introuvable.");
});

app.Run();

#region Helper Functions

static ShotResultDto ExecuteAiTurn(Game game)
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

static GameStateDto ToDto(Game game)
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
        new BoardDto(game.AiBoard.Shots, aiHits, aiMisses, null)
    );
}

static ShotResultDto ProcessShot(Board targetBoard, Position target)
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

static Board GenerateRandomAiBoard()
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