using Battleship.Contracts;
using FluentValidation;
namespace Battleship.Validators;
public class TakeShotRequestValidator : AbstractValidator<TakeShotRequest>
{
    public TakeShotRequestValidator()
    {
        RuleFor(x => x.Target)
            .NotNull().WithMessage("La position cible est obligatoire.");

        RuleFor(x => x.Target.Row)
            .InclusiveBetween(0, 9).WithMessage("La ligne doit être comprise entre 0 et 9.");

        RuleFor(x => x.Target.Column)
            .InclusiveBetween(0, 9).WithMessage("La colonne doit être comprise entre 0 et 9.");
    }
}