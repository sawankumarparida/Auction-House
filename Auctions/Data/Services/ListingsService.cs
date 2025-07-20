using Auctions.Models;
using Microsoft.EntityFrameworkCore;

namespace Auctions.Data.Services
{
    public class ListingsService : IListingsService
    {
        private readonly ApplicationDbContext _context;

        public ListingsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Add(Listing listing)
        {
            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();
        }

        public Task Delete(Listing listing)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Listing> GetAll()
        {
            var applicationDbContext = _context.Listings.Include(l => l.User);
            return applicationDbContext;
        }

        public async Task<Listing> GetById(int? id)
        {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

            var listing = await _context.Listings
                .Include(l => l.User)
                .Include(l => l.Comments)
                .Include(l => l.Bids)
                .ThenInclude(l => l.User)
                .FirstOrDefaultAsync(m => m.Id == id);
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

#pragma warning disable CS8603 // Possible null reference return.

            return listing;
#pragma warning restore CS8603 // Possible null reference return.

        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        public Task Update(Listing listing)
        {
            throw new NotImplementedException();
        }
    }
}
