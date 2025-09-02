using API.APIService;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace BlazorCocktails.Client.Shared.Validators;

public class CredentialsUserDTOValidator : AbstractValidator<CredentialsUserDTO>
{
    public CredentialsUserDTOValidator(IStringLocalizer<App> L)
    {
        RuleFor(x => x.Email)
          .NotEmpty().WithMessage(L["Login_EmailRequired"])
          .EmailAddress().WithMessage(L["Login_EmailInvalid"]);

        RuleFor(x => x.Password)
          .NotEmpty().WithMessage(L["Login_PasswordRequired"]);
    }
}
