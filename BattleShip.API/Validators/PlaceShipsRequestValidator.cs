using Battleship.Contracts;
using FluentValidation;
namespace Battleship.Validators;

public class PlaceShipsRequestValidator : AbstractValidator<PlaceShipsRequest>
{
    public PlaceShipsRequestValidator()
    {
        RuleFor(x => x.Ships)
            .NotNull().WithMessage("La liste des navires est obligatoire.")
            .Must(ships => ships != null && ships.Count == 5)
            .WithMessage("Vous devez placer exactement 5 navires.");

        RuleForEach(x => x.Ships).ChildRules(ship =>
        {
            ship.RuleFor(s => s.Position.Row)
                .InclusiveBetween(0, 9).WithMessage("La ligne du navire doit être entre 0 et 9.");

            ship.RuleFor(s => s.Position.Column)
                .InclusiveBetween(0, 9).WithMessage("La colonne du navire doit être entre 0 et 9.");

            ship.RuleFor(s => s.Direction)
                .IsInEnum().WithMessage("La direction du navire est invalide.");
        });
    }
}