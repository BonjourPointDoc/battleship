using BattleShip.Models;
using Battleship.Contracts;
using Battleship.Services;
using FluentAssertions;
using Xunit;
namespace BattleShip.Tests;

public class BattleshipAiServiceTests
{
    private readonly IBattleshipAiService _aiService;
    private readonly IGameEngineService _engineService;

    public BattleshipAiServiceTests()
    {
        _engineService = new GameEngineService();
        _aiService = new BattleshipAiService(_engineService);
    }

    [Fact]
    public void GenerateRandomBoard_ShouldPlaceAllShipsWithinGridWithoutOverlap()
    {
        // Act
        var board = _aiService.GenerateRandomBoard();

        // Assert
        var totalExpectedShips = Enum.GetValues<ShipType>().Length;
        board.Ships.Should().HaveCount(totalExpectedShips);

        var allPositions = board.Ships.SelectMany(s => s.GetPositions()).ToList();

        // Aucune case hors grille
        allPositions.Should().OnlyContain(p => _engineService.IsValidPosition(p));

        // Aucune chevauchement
        allPositions.Distinct().Count().Should().Be(allPositions.Count);
    }

    [Fact]
    public void SelectAiTarget_WhenUnsunkHitExists_TargetsAdjacentPosition()
    {
        // Arrange
        var ship = new Ship(ShipType.Cruiser, new Position(5, 5), Direction.Horizontal); // (5,5), (5,6), (5,7)
        var board = new Board { Ships = new List<Ship> { ship } }
            .WithShot(new Position(5, 5)); // Touché mais non coulé

        // Act
        var target = _aiService.SelectAiTarget(board);

        // Assert
        var validNeighbors = new[]
        {
            new Position(4, 5),
            new Position(6, 5),
            new Position(5, 4),
            new Position(5, 6)
        };

        validNeighbors.Should().Contain(target);
    }

    [Fact]
    public void ExecuteAiTurn_ShouldRegisterShotAndSwitchTurnIfNotOver()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var aiId = Guid.NewGuid();
        var game = new Game
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            AiId = aiId,
            CurrentPlayerId = aiId,
            PlayerBoard = new Board
            {
                Ships = new List<Ship>
                {
                    new Ship(ShipType.Destroyer, new Position(0, 0), Direction.Horizontal)
                }
            },
            AiBoard = new Board(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var shotResult = _aiService.ExecuteAiTurn(game);

        // Assert
        shotResult.Should().NotBeNull();
        game.PlayerBoard.Shots.Should().HaveCount(1);
        game.CurrentPlayerId.Should().Be(playerId); // La main repasse au joueur
    }
}