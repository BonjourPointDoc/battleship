using Battleship.Contracts;
using Battleship.Validators;
using BattleShip.Models;
using FluentValidation.TestHelper;
using Xunit;

public class TakeShotRequestValidatorTests
{
    private readonly TakeShotRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Row_Is_Out_Of_Bounds()
    {
        // Arrange
        var request = new TakeShotRequest(new Position(15, 5));

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Target.Row)
              .WithErrorMessage("La ligne doit être comprise entre 0 et 9.");
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(5, 5)]
    [InlineData(9, 9)]
    public void Should_Not_Have_Error_When_Target_Is_Valid(int row, int col)
    {
        // Arrange
        var request = new TakeShotRequest(new Position(row, col));

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}