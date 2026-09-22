using System.Net;
using System.Net.Http.Json;
using Battleship.Contracts;
using BattleShip.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BattleShip.Tests;

public class GamesApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GamesApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateGame_ReturnsCreatedAndValidInitialState()
    {
        // Act
        var response = await _client.PostAsync("/api/games", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var game = await response.Content.ReadFromJsonAsync<GameStateDto>();
        game.Should().NotBeNull();
        game!.Id.Should().NotBeEmpty();
        game.Status.Should().Be(GameState.WaitingForPlayerBoard);
        game.PlayerBoard.Shots.Should().BeEmpty();
    }

    [Fact]
    public async Task GetGame_WhenExists_ReturnsOk()
    {
        // Arrange
        var createRes = await _client.PostAsync("/api/games", null);
        var createdGame = await createRes.Content.ReadFromJsonAsync<GameStateDto>();

        // Act
        var response = await _client.GetAsync($"/api/games/{createdGame!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetchedGame = await response.Content.ReadFromJsonAsync<GameStateDto>();
        fetchedGame!.Id.Should().Be(createdGame.Id);
    }

    [Fact]
    public async Task GetGame_WhenNotFound_Returns404()
    {
        // Act
        var response = await _client.GetAsync($"/api/games/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PlaceShips_WhenValid_ReturnsOk()
    {
        // Arrange
        var createRes = await _client.PostAsync("/api/games", null);
        var game = await createRes.Content.ReadFromJsonAsync<GameStateDto>();

        var shipsRequest = new PlaceShipsRequest(new List<Ship>
        {
            new Ship(ShipType.Carrier, new Position(0, 0), Direction.Horizontal),
            new Ship(ShipType.Battleship, new Position(1, 0), Direction.Horizontal),
            new Ship(ShipType.Cruiser, new Position(2, 0), Direction.Horizontal),
            new Ship(ShipType.Submarine, new Position(3, 0), Direction.Horizontal),
            new Ship(ShipType.Destroyer, new Position(4, 0), Direction.Horizontal)
        });

        // Act
        var response = await _client.PostAsJsonAsync($"/api/games/{game!.Id}/board", shipsRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PlaceShips_WhenAlreadyPlaced_ReturnsBadRequest()
    {
        // Arrange
        var createRes = await _client.PostAsync("/api/games", null);
        var game = await createRes.Content.ReadFromJsonAsync<GameStateDto>();

        var shipsRequest = new PlaceShipsRequest(new List<Ship>
        {
            new Ship(ShipType.Destroyer, new Position(0, 0), Direction.Horizontal)
        });

        // Premier placement
        await _client.PostAsJsonAsync($"/api/games/{game!.Id}/board", shipsRequest);

        // Act : Deuxième placement
        var secondResponse = await _client.PostAsJsonAsync($"/api/games/{game.Id}/board", shipsRequest);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TakeShot_WhenValidTurn_ReturnsOkWithTurnResult()
    {
        // 1. Création de la partie
        var createRes = await _client.PostAsync("/api/games", null);
        var game = await createRes.Content.ReadFromJsonAsync<GameStateDto>();

        // 2. Placement des bateaux
        var shipsRequest = new PlaceShipsRequest(new List<Ship>
        {
            new Ship(ShipType.Carrier, new Position(0, 0), Direction.Horizontal),
            new Ship(ShipType.Battleship, new Position(1, 0), Direction.Horizontal),
            new Ship(ShipType.Cruiser, new Position(2, 0), Direction.Horizontal),
            new Ship(ShipType.Submarine, new Position(3, 0), Direction.Horizontal),
            new Ship(ShipType.Destroyer, new Position(4, 0), Direction.Horizontal)
        });
        await _client.PostAsJsonAsync($"/api/games/{game!.Id}/board", shipsRequest);

        // 3. Récupération de l'état actuel pour vérifier à qui est le tour
        var updatedGameRes = await _client.GetAsync($"/api/games/{game.Id}");
        var updatedGame = await updatedGameRes.Content.ReadFromJsonAsync<GameStateDto>();

        // S'assurer que le joueur a la main
        if (updatedGame!.CurrentPlayerId == updatedGame.PlayerId)
        {
            // Act
            var shotReq = new TakeShotRequest(new Position(0, 0));
            var response = await _client.PostAsJsonAsync($"/api/games/{game.Id}/shots", shotReq);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var turnResult = await response.Content.ReadFromJsonAsync<TurnResponseDto>();
            turnResult.Should().NotBeNull();
            // ✅ Après (selon le nom de la propriété dans TurnResponseDto)
            turnResult!.PlayerShotResult.Position.Should().Be(new Position(0, 0));
        }
    }

    [Fact]
    public async Task DeleteGame_RemovesGameFromStore()
    {
        // Arrange
        var createRes = await _client.PostAsync("/api/games", null);
        var game = await createRes.Content.ReadFromJsonAsync<GameStateDto>();

        // Act
        var deleteRes = await _client.DeleteAsync($"/api/games/{game!.Id}");

        // Assert
        deleteRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var getRes = await _client.GetAsync($"/api/games/{game.Id}");
        getRes.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}