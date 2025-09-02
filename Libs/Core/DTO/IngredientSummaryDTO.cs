using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.DTO
{
    public class IngredientSummaryDTO
    {
        [JsonPropertyName("strIngredient1")]
        public string? Name { get; set; }
    }
}
