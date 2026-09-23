extern alias AppAssembly;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using BattleShip.App.Pages;
using BattleShip.App.Services;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using Xunit;

using BoardDto = BattleShip.App.Services.BoardDto;
using GameStateDto = BattleShip.App.Services.GameStateDto;

using BattleshipGrpc = AppAssembly::Battleship.Grpc.BattleshipGrpc;
using Grpc.Net.Client;

namespace BattleShip.Tests.App;

public class GameTests : TestContext
{
    private readonly MockHttpMessageHandler _mockHttp;
    private static readonly JsonSerializerOptions JsonWebOptions = new(JsonSerializerDefaults.Web);

    public GameTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        var httpClient = _mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost/");

        Services.AddSingleton(httpClient);

        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
        {
            HttpClient = httpClient
        });

        var grpcClient = new BattleshipGrpc.BattleshipGrpcClient(channel);
        Services.AddSingleton(grpcClient);

        Services.AddScoped<GameService>();
    }

    private static GameStateDto CreateInProgressGameState(Guid gameId) => new()
    {
        Id = gameId,
        PlayerId = Guid.NewGuid(),
        AiId = Guid.NewGuid(),
        CurrentPlayerId = gameId,
        Status = 1, // 1 = InProgress
        WinnerId = null,
        PlayerBoard = new BoardDto { Shots = [], Hits = [], Misses = [], Ships = [] },
        AiBoard = new BoardDto { Shots = [], Hits = [], Misses = [], Ships = null },
        CreatedAt = DateTimeOffset.UtcNow
    };

    protected override void Dispose(bool disposing)
    {
        _mockHttp.Dispose();
        base.Dispose(disposing);
    }
}