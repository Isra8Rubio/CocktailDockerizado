using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.DTO
{
    public class IngredientDetailRawDTO
    {
        [JsonPropertyName("idIngredient")]
        public string? IdIngredient { get; set; }

        [JsonPropertyName("strIngredient")]
        public string? Name { get; set; }

        [JsonPropertyName("strType")]
        public string? Type { get; set; }

        [JsonPropertyName("strDescription")]
        public string? Description { get; set; }
    }
}
