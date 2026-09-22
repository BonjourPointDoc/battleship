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

namespace BattleShip.Tests.App;

public class HomeTests : TestContext
{
    private readonly MockHttpMessageHandler _mockHttp = new();

    public HomeTests()
    {
        var httpClient = _mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost/"); // <--- CAPITAL : définit la base d'URL
        Services.AddSingleton(httpClient);
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

    [Fact]
    public void Home_AfficheListeDesParties()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var gamesList = new List<GameStateDto> { CreateGameStateDto(gameId, status: 1) };

        // Accepte n'importe quelle variante d'URL contenant /api/games
        _mockHttp.When(HttpMethod.Get, "*api/games*")
                 .Respond("application/json", JsonSerializer.Serialize(gamesList));

        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".game-item").Should().HaveCount(1);
            cut.Find(".game-status").TextContent.Should().Contain("En cours");
        });
    }

    protected override void Dispose(bool disposing)
    {
        _mockHttp.Dispose();
        base.Dispose(disposing);
    }
}