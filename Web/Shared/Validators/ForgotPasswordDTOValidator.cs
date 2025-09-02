using API.APIService;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace BlazorCocktails.Client.Shared.Validators;

public class ForgotPasswordDTOValidator : AbstractValidator<ForgotPasswordDTO>
{
    public ForgotPasswordDTOValidator(IStringLocalizer<App> L)
    {
        RuleFor(x => x.Email)
          .NotEmpty().WithMessage(L["Register_EmailRequired"])
          .EmailAddress().WithMessage(L["Register_EmailInvalid"]);
    }
}
