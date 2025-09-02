using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class RandomCocktail
    {
        public int Id { get; set; }
        public string DrinkId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string ThumbUrl { get; set; } = default!;
    }
}
