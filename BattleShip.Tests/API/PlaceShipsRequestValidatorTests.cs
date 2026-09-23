using Battleship.Contracts;
using BattleShip.Models;
using Battleship.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace Battleship.Tests.Validators;

public class PlaceShipsRequestValidatorTests
{
    private readonly PlaceShipsRequestValidator _validator = new();

    private static List<Ship> CreateValidShips() => new()
    {
        new Ship(ShipType.Carrier, new Position(0, 0), Direction.Horizontal),
        new Ship(ShipType.Battleship, new Position(1, 0), Direction.Horizontal),
        new Ship(ShipType.Cruiser, new Position(2, 0), Direction.Horizontal),
        new Ship(ShipType.Submarine, new Position(3, 0), Direction.Horizontal),
        new Ship(ShipType.Destroyer, new Position(4, 0), Direction.Horizontal)
    };

    [Fact]
    public void Should_Have_Error_When_Ships_Is_Null()
    {
        var request = new PlaceShipsRequest(null!);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Ships)
              .WithErrorMessage("La liste des navires est obligatoire.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(6)]
    public void Should_Have_Error_When_Ships_Count_Is_Not_Five(int count)
    {

        var ships = Enumerable.Range(0, count)
            .Select(i => new Ship(ShipType.Destroyer, new Position(0, 0), Direction.Horizontal))
            .ToList();

        var request = new PlaceShipsRequest(ships);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Ships)
            .WithErrorMessage("Vous devez placer exactement 5 navires.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10)]
    public void Should_Have_Error_When_Ship_Row_Is_Out_Of_Bounds(int invalidRow)
    {
        var ships = CreateValidShips();
        ships[0] = new Ship(ShipType.Carrier, new Position(invalidRow, 0), Direction.Horizontal);
        var request = new PlaceShipsRequest(ships);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Ships[0].Position.Row")
              .WithErrorMessage("La ligne du navire doit être entre 0 et 9.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10)]
    public void Should_Have_Error_When_Ship_Column_Is_Out_Of_Bounds(int invalidCol)
    {
        var ships = CreateValidShips();
        ships[0] = new Ship(ShipType.Carrier, new Position(0, invalidCol), Direction.Horizontal);
        var request = new PlaceShipsRequest(ships);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Ships[0].Position.Column")
              .WithErrorMessage("La colonne du navire doit être entre 0 et 9.");
    }

    [Fact]
    public void Should_Have_Error_When_Ship_Direction_Is_Invalid()
    {
        var ships = CreateValidShips();
        ships[0] = new Ship(ShipType.Carrier, new Position(0, 0), (Direction)99);
        var request = new PlaceShipsRequest(ships);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Ships[0].Direction")
              .WithErrorMessage("La direction du navire est invalide.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        var request = new PlaceShipsRequest(CreateValidShips());

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}