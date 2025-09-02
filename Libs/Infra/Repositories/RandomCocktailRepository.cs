using Core.Entities;
using Infraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Repositories
{
    public class RandomCocktailRepository
    {
        private readonly ApplicationDbContext _ctx;
        public RandomCocktailRepository(ApplicationDbContext ctx) => _ctx = ctx;

        public async Task<RandomCocktail?> GetAsync(CancellationToken ct = default)
            => await _ctx.RandomCocktails.AsNoTracking()
                   .OrderBy(x => x.Id)
                   .FirstOrDefaultAsync(ct);

        public async Task AddOrUpdateAsync(RandomCocktail row, CancellationToken ct = default)
        {
            var existing = await _ctx.RandomCocktails
                                     .OrderBy(x => x.Id)
                                     .FirstOrDefaultAsync(ct);

            if (existing is null)
            {
                _ctx.RandomCocktails.Add(row);
            }
            else
            {
                existing.DrinkId = row.DrinkId;
                existing.Name = row.Name;
                existing.ThumbUrl = row.ThumbUrl;
            }

            await _ctx.SaveChangesAsync(ct);
        }
    }
}
