using Core.DTO;
using FluentValidation;

public class ForgotPasswordDTOValidator : AbstractValidator<ForgotPasswordDTO>
{
    public ForgotPasswordDTOValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("Formato de email inválido.");
    }
}
