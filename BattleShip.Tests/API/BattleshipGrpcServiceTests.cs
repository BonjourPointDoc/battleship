using Battleship.Grpc;
using Battleship.Services;
using BattleShip.Models;
using FluentAssertions;
using Grpc.Core;
using Moq;
using Xunit;

namespace Battleship.Tests;

[CollectionDefinition("GrpcTests", DisableParallelization = true)]
public class GrpcTestCollection : ICollectionFixture<object> { }

[Collection("GrpcTests")]
public class BattleshipGrpcServiceTests : IAsyncLifetime
{
    private readonly BattleshipGrpcService _service;
    private readonly ServerCallContext _context;

    public BattleshipGrpcServiceTests()
    {
        _service = new BattleshipGrpcService();
        _context = new Mock<ServerCallContext>().Object;
    }

    public Task InitializeAsync()
    {
        // Nettoyage complet du stockage statique avant CHAQUE test
        return _service.DeleteAllGames(new Empty(), _context);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateGame_ShouldReturnValidGameState_WithoutNullWinnerId()
    {
        // Act
        var result = await _service.CreateGame(new Empty(), _context);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.Status.Should().Be(GameState.WaitingForPlayerBoard.ToString());
        result.WinnerId.Should().BeEmpty(); // Doit être vide (chaîne vide Protobuf par défaut) sans lever d'exception null
        result.PlayerBoard.Ships.Should().BeEmpty();
        result.AiBoard.Shots.Should().BeEmpty();
    }

    [Fact]
    public async Task GetGame_WhenGameDoesNotExist_ShouldThrowNotFoundRpcException()
    {
        // Arrange
        var request = new GetGameRequest { Id = Guid.NewGuid().ToString() };

        // Act
        Func<Task> act = async () => await _service.GetGame(request, _context);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task PlaceShips_WhenValidRequest_ShouldUpdatePlayerBoard()
    {
        // Arrange
        var createdGame = await _service.CreateGame(new Empty(), _context);
        var request = new PlaceShipsGrpcRequest
        {
            GameId = createdGame.Id,
            Ships =
            {
                new ShipMessage
                {
                    Type = ShipType.Destroyer.ToString(),
                    Direction = Direction.Horizontal.ToString(),
                    BowPosition = new PositionMessage { Row = 0, Column = 0 }
                }
            }
        };

        // Act
        var response = await _service.PlaceShips(request, _context);

        // Assert
        response.Game.PlayerBoard.Ships.Should().HaveCount(1);
        response.Game.PlayerBoard.Ships[0].Type.Should().Be(ShipType.Destroyer.ToString());
    }

    [Fact]
    public async Task PlaceShips_WhenShipsAlreadyPlaced_ShouldThrowInvalidArgument()
    {
        // Arrange
        var createdGame = await _service.CreateGame(new Empty(), _context);
        var request = new PlaceShipsGrpcRequest
        {
            GameId = createdGame.Id,
            Ships =
            {
                new ShipMessage
                {
                    Type = ShipType.Destroyer.ToString(),
                    Direction = Direction.Horizontal.ToString(),
                    BowPosition = new PositionMessage { Row = 0, Column = 0 }
                }
            }
        };

        await _service.PlaceShips(request, _context);

        // Act
        Func<Task> act = async () => await _service.PlaceShips(request, _context);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task TakeShot_WhenValidShot_ShouldReturnTurnResponse()
    {
        // Arrange
        var createdGame = await _service.CreateGame(new Empty(), _context);
        
        // Placer un bateau pour débloquer la partie
        await _service.PlaceShips(new PlaceShipsGrpcRequest
        {
            GameId = createdGame.Id,
            Ships =
            {
                new ShipMessage
                {
                    Type = ShipType.Submarine.ToString(),
                    Direction = Direction.Horizontal.ToString(),
                    BowPosition = new PositionMessage { Row = 0, Column = 0 }
                }
            }
        }, _context);

        // Récupérer l'état à jour pour vérifier à qui est le tour
        var updatedGame = await _service.GetGame(new GetGameRequest { Id = createdGame.Id }, _context);

        if (updatedGame.CurrentPlayerId == updatedGame.PlayerId)
        {
            var shotRequest = new TakeShotGrpcRequest
            {
                GameId = createdGame.Id,
                Target = new PositionMessage { Row = 5, Column = 5 }
            };

            // Act
            var response = await _service.TakeShot(shotRequest, _context);

            // Assert
            response.Should().NotBeNull();
            response.PlayerShotResult.Should().NotBeNull();
            response.PlayerShotResult.Target.Row.Should().Be(5);
            response.PlayerShotResult.Target.Column.Should().Be(5);
        }
    }

    [Fact]
    public async Task DeleteGame_WhenExists_ShouldRemoveGame()
    {
        // Arrange
        var game = await _service.CreateGame(new Empty(), _context);

        // Act
        var deleteResponse = await _service.DeleteGame(new DeleteGameRequest { Id = game.Id }, _context);

        // Assert
        deleteResponse.Message.Should().Contain(game.Id);

        Func<Task> getAct = async () => await _service.GetGame(new GetGameRequest { Id = game.Id }, _context);
        await getAct.Should().ThrowAsync<RpcException>().Where(e => e.StatusCode == StatusCode.NotFound);
    }
}