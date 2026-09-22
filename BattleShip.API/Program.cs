using System.Collections.Concurrent;
using Battleship.Contracts;
using BattleShip.Models;
using Battleship.Services;
using Battleship.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Injection des services métiers
builder.Services.AddSingleton<IGameEngineService, GameEngineService>();
builder.Services.AddSingleton<IBattleshipAiService, BattleshipAiService>();
builder.Services.AddSingleton<ConcurrentDictionary<Guid, Game>>();

builder.Services.AddGrpc();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
    });
});

var app = builder.Build();

app.UseRouting();
app.UseCors("DevCorsPolicy");
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

app.MapGrpcService<BattleshipGrpcService>()
   .EnableGrpcWeb()
   .RequireCors("DevCorsPolicy");

var api = app.MapGroup("/api/games");

api.MapGet("/", (ConcurrentDictionary<Guid, Game> games, IGameEngineService engine) =>
{
    var allGames = games.Values
        .OrderByDescending(g => g.CreatedAt)
        .Select(engine.ToDto)
        .ToList();

    return Results.Ok(allGames);
});

api.MapPost("/", (ConcurrentDictionary<Guid, Game> games, IBattleshipAiService ai, IGameEngineService engine) =>
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
        AiBoard = ai.GenerateRandomBoard(),
        PlayerBoard = new Board(),
        CreatedAt = DateTimeOffset.UtcNow
    };

    games[game.Id] = game;

    return Results.Created($"/api/games/{game.Id}", engine.ToDto(game));
});

api.MapGet("/{id:guid}", (Guid id, ConcurrentDictionary<Guid, Game> games, IGameEngineService engine) =>
{
    return games.TryGetValue(id, out var game)
        ? Results.Ok(engine.ToDto(game))
        : Results.NotFound("Partie introuvable.");
});

api.MapPost("/{id:guid}/board", (
    Guid id,
    [FromBody] PlaceShipsRequest request,
    ConcurrentDictionary<Guid, Game> games,
    IBattleshipAiService ai,
    IGameEngineService engine) =>
{
    if (!games.TryGetValue(id, out var game))
        return Results.NotFound("Partie introuvable.");

    if (game.PlayerBoard.Ships.Count > 0)
        return Results.BadRequest("Les bateaux sont déjà placés.");

    game.PlayerBoard = game.PlayerBoard.WithShips(request.Ships);

    ShotResultDto? initialAiShot = null;
    if (game.CurrentPlayerId == game.AiId)
    {
        initialAiShot = ai.ExecuteAiTurn(game);
    }

    return Results.Ok(new
    {
        Game = engine.ToDto(game),
        InitialAiShot = initialAiShot
    });
}).Validate<PlaceShipsRequest>();

api.MapPost("/{id:guid}/shots", (
    Guid id,
    [FromBody] TakeShotRequest request,
    ConcurrentDictionary<Guid, Game> games,
    IBattleshipAiService ai,
    IGameEngineService engine) =>
{
    if (!games.TryGetValue(id, out var game))
        return Results.NotFound("Partie introuvable.");

    if (game.PlayerBoard.IsGameOver() || game.AiBoard.IsGameOver())
        return Results.BadRequest("La partie est terminée.");

    if (game.CurrentPlayerId != game.PlayerId)
        return Results.BadRequest("Ce n'est pas votre tour.");

    if (game.AiBoard.IsShot(request.Target))
        return Results.BadRequest("Cette case a déjà été ciblée.");

    game.AiBoard = game.AiBoard.WithShot(request.Target);
    var playerShotResult = engine.ProcessShot(game.AiBoard, request.Target);

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
    var aiShotResult = ai.ExecuteAiTurn(game);

    var winnerId = game.PlayerBoard.IsGameOver() ? game.AiId : (Guid?)null;
    var status = winnerId.HasValue ? GameState.Finished : GameState.InProgress;

    return Results.Ok(new TurnResponseDto(
        playerShotResult,
        aiShotResult,
        status,
        game.CurrentPlayerId,
        winnerId
    ));
}).Validate<TakeShotRequest>();

api.MapDelete("/", (ConcurrentDictionary<Guid, Game> games) =>
{
    games.Clear();
    return Results.Ok(new { Message = "Toutes les parties ont été supprimées." });
});

api.MapDelete("/{id:guid}", (Guid id, ConcurrentDictionary<Guid, Game> games) =>
{
    return games.TryRemove(id, out _)
        ? Results.Ok(new { Message = $"La partie {id} a été supprimée." })
        : Results.NotFound("Partie introuvable.");
});

app.Run();

public partial class Program {}