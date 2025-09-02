using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.DTO
{
    public class CocktailDetailDTO
    {
        public string? IdDrink { get; set; }
        public string? StrDrink { get; set; }
        public string? StrCategory { get; set; }
        public string? StrInstructionsES { get; set; }
        public string? StrAlcoholic { get; set; }
        public string? StrGlass { get; set; }
        public string? StrInstructions { get; set; }
        public string? StrDrinkThumb { get; set; }
        public List<IngredientDTO>? Ingredients { get; set; }
    }
}
