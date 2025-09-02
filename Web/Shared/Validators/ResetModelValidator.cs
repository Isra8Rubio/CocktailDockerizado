using BlazorCocktails.Client;
using BlazorCocktails.Client.Models;
using FluentValidation;
using Microsoft.Extensions.Localization;

public class ResetModelValidator : AbstractValidator<ResetModel>
{
    public ResetModelValidator(IStringLocalizer<App> L)
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(L["Login_PasswordRequired"])
            .MinimumLength(6).WithMessage(L["Register_PasswordMin"]);

        RuleFor(x => x.Confirm)
            .NotEmpty().WithMessage(L["Register_ConfirmRequired"])
            .Equal(x => x.NewPassword).WithMessage(L["Register_PasswordsDontMatch"]);
    }
}
