using API.APIService;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace BlazorCocktails.Client.Shared.Validators;

public class RegisterUserDTOValidator : AbstractValidator<RegisterUserDTO>
{
    public RegisterUserDTOValidator(IStringLocalizer<App> L)
    {
        RuleFor(x => x.Email)
          .NotEmpty().WithMessage(L["Register_EmailRequired"])
          .EmailAddress().WithMessage(L["Register_EmailInvalid"]);

        RuleFor(x => x.Password)
          .NotEmpty().WithMessage(L["Register_PasswordRequired"])
          .MinimumLength(6).WithMessage(L["Register_PasswordMin"]);

        RuleFor(x => x.ConfirmPassword)
          .NotEmpty().WithMessage(L["Register_ConfirmRequired"])
          .Equal(x => x.Password).WithMessage(L["Register_PasswordsDontMatch"]);
    }
}
