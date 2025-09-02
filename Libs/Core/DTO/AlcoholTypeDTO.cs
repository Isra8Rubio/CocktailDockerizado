using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.DTO
{
    public class AlcoholTypeDTO
    {
        [JsonPropertyName("strAlcoholic")]
        public string? StrAlcoholic { get; set; }
    }
}
