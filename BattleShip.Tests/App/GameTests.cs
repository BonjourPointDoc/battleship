using System;
using System.Net.Http;
using System.Text.Json;
using BattleShip.App.Pages;
using BattleShip.App.Services;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using Xunit;

using BoardDto = BattleShip.App.Services.BoardDto;
using GameStateDto = BattleShip.App.Services.GameStateDto;

namespace BattleShip.Tests.App;

public class GameTests : TestContext
{
    private readonly MockHttpMessageHandler _mockHttp = new();

    public GameTests()
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
    public void Game_QuandNouvellePartie_AffichePlateauDePlacement()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = CreateGameStateDto(gameId, status: 0);

        _mockHttp.When(HttpMethod.Post, "*api/games*")
                 .Respond("application/json", JsonSerializer.Serialize(game));

        _mockHttp.When(HttpMethod.Get, $"*api/games/{gameId}*")
                 .Respond("application/json", JsonSerializer.Serialize(game));

        // Act
        var cut = RenderComponent<BattleShip.App.Pages.Game>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find("h2").TextContent.Should().Contain("Placez vos bateaux");
        });
    }

    [Fact]
    public void Game_QuandPartieEnCours_AfficheInterfaceDeCombat()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = CreateGameStateDto(gameId, status: 1);

        _mockHttp.When(HttpMethod.Get, $"*api/games/{gameId}*")
                 .Respond("application/json", JsonSerializer.Serialize(game));

        // Définir l'URL avec le Query Parameter via le NavigationManager
        var navManager = Services.GetRequiredService<NavigationManager>();
        navManager.NavigateTo($"http://localhost/game?id={gameId}");

        // Act
        var cut = RenderComponent<BattleShip.App.Pages.Game>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".battle-status").Should().NotBeNull();
        });
    }

    protected override void Dispose(bool disposing)
    {
        _mockHttp.Dispose();
        base.Dispose(disposing);
    }
}