using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.DTO
{
    public class GlassResponseDTO
    {
        [JsonPropertyName("drinks")]
        public List<GlassDTO>? Drinks { get; set; }
    }
}
