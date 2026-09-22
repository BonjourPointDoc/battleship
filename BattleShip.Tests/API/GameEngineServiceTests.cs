using BattleShip.Models;
using Battleship.Contracts;
using Battleship.Services;
using FluentAssertions;
using Xunit;
namespace BattleShip.Tests;

public class GameEngineServiceTests
{
    private readonly GameEngineService _engine = new();

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(9, 9, true)]
    [InlineData(-1, 0, false)]
    [InlineData(0, -1, false)]
    [InlineData(10, 5, false)]
    [InlineData(5, 10, false)]
    public void IsValidPosition_ShouldValidateGridBounds(int row, int col, bool expected)
    {
        // Act
        var result = _engine.IsValidPosition(new Position(row, col));

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ProcessShot_WhenHittingShipPartially_ReturnsHitNotSunk()
    {
        // Arrange
        var ship = new Ship(ShipType.Destroyer, new Position(0, 0), Direction.Horizontal); // Taille 2 : (0,0) et (0,1)
        var board = new Board { Ships = new List<Ship> { ship } }.WithShot(new Position(0, 0));

        // Act
        var result = _engine.ProcessShot(board, new Position(0, 0));

        // Assert
        result.IsHit.Should().BeTrue();
        result.IsSunk.Should().BeFalse();
        result.SunkShipType.Should().BeNull();
    }

    [Fact]
    public void ProcessShot_WhenAllPositionsHit_ReturnsSunkWithShipType()
    {
        // Arrange
        var ship = new Ship(ShipType.Destroyer, new Position(0, 0), Direction.Horizontal);
        var board = new Board { Ships = new List<Ship> { ship } }
            .WithShot(new Position(0, 0))
            .WithShot(new Position(0, 1));

        // Act
        var result = _engine.ProcessShot(board, new Position(0, 1));

        // Assert
        result.IsHit.Should().BeTrue();
        result.IsSunk.Should().BeTrue();
        result.SunkShipType.Should().Be(ShipType.Destroyer);
    }

    [Fact]
    public void ToDto_ShouldMapGameCorrectly()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var aiId = Guid.NewGuid();
        var game = new Game
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            AiId = aiId,
            CurrentPlayerId = playerId,
            PlayerBoard = new Board(),
            AiBoard = new Board(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var dto = _engine.ToDto(game);

        // Assert
        dto.Id.Should().Be(game.Id);
        dto.PlayerId.Should().Be(playerId);
        dto.AiId.Should().Be(aiId);
        dto.Status.Should().Be(GameState.WaitingForPlayerBoard);
        dto.WinnerId.Should().BeNull();
    }
}
