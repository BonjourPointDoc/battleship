using BattleShip.App.Services;
using BattleShip.Models;
using FluentAssertions;
using Xunit;

namespace BattleShip.Tests.Services;

public class GameServiceTests
{
    [Fact]
    public void GameService_Should_Initialize_With_Correct_Default_Values()
    {
        // Arrange & Act
        // On passe null car nous ne testons pas les appels gRPC ici[cite: 3]
        var service = new GameService(null); 

        // Assert
        service.IsLoading.Should().BeTrue();
        service.GameStarted.Should().BeFalse();
        service.PlacementFinished.Should().BeFalse();
        service.CurrentDirection.Should().Be(Direction.Horizontal);
        service.CurrentShipType.Should().Be(GameService.ShipTypes[0]); 
    }

    [Fact]
    public void OnCellRightClicked_Should_Toggle_Direction()
    {
        // Arrange
        var service = new GameService(null);
        var initialDirection = service.CurrentDirection;
        var position = new Position(2, 3);

        // Act
        service.OnCellRightClicked(position); //[cite: 3]

        // Assert
        service.CurrentDirection.Should().NotBe(initialDirection);
        service.CurrentDirection.Should().Be(Direction.Vertical);
        service.HoveredPosition.Should().Be(position);
    }

    [Fact]
    public void OnCellMouseEnter_Should_Update_HoveredPosition_If_Placement_Not_Finished()
    {
        // Arrange
        var service = new GameService(null);
        var position = new Position(5, 5);

        // Act
        service.OnCellMouseEnter(position); //[cite: 3]

        // Assert
        service.HoveredPosition.Should().Be(position);
    }
}