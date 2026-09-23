extern alias AppAssembly;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
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

public class HomeTests : TestContext
{
    private readonly MockHttpMessageHandler _mockHttp;
    private static readonly JsonSerializerOptions JsonWebOptions = new(JsonSerializerDefaults.Web);

    public HomeTests()
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

    private static BoardDto CreateEmptyBoardDto() => new()
    {
        Shots = [],
        Hits = [],
        Misses = [],
        Ships = null
    };

    private static GameStateDto CreateGameStateDto(Guid id, int status = 0) => new()
    {
        Id = id,
        PlayerId = Guid.NewGuid(),
        AiId = Guid.NewGuid(),
        CurrentPlayerId = id,
        Status = status,
        WinnerId = null,
        PlayerBoard = CreateEmptyBoardDto(),
        AiBoard = CreateEmptyBoardDto(),
        CreatedAt = DateTimeOffset.UtcNow
    };

    protected override void Dispose(bool disposing)
    {
        _mockHttp.Dispose();
        base.Dispose(disposing);
    }
}