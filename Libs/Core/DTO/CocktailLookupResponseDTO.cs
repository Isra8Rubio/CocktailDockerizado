using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.DTO
{
    public class CocktailLookupResponseDTO
    {
        [JsonPropertyName("drinks")]
        public List<CocktailDetailRawDTO>? Drinks { get; set; }
    }
}
