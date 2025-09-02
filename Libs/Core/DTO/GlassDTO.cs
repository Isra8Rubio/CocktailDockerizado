using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.DTO
{
    public class GlassDTO
    {
        [JsonPropertyName("strGlass")]
        public string? StrGlass { get; set; }
    }
}
