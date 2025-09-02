using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.DTO
{
    public class AnswerAuthenticationDTO
    {
        public string? Token { get; set; }
        [JsonIgnore]
        public DateTime Expiration { get; set; }
    }
}
