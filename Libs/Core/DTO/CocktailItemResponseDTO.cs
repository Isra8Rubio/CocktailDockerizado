using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.DTO
{
    public class CocktailItemResponseDTO
    {
        [JsonPropertyName("drinks")]
        public List<CocktailItemDTO>? Drinks { get; set; }
    }
}
