using Core.DTO;
using FluentValidation;

namespace Core.Validators
{
    public class CredentialsUserDTOValidator : AbstractValidator<CredentialsUserDTO>
    {
        public CredentialsUserDTOValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
        }
    }
}
