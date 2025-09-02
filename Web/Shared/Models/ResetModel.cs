namespace BlazorCocktails.Client.Models;

public class ResetModel
{
    public string Email { get; set; } = "";
    public string Token { get; set; } = "";
    public string NewPassword { get; set; } = "";
    public string Confirm { get; set; } = "";
}
