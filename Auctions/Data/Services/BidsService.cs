﻿using Auctions.Models;
using Microsoft.EntityFrameworkCore;

namespace Auctions.Data.Services
{
    public class BidsService : IBidsService
    {
        private readonly ApplicationDbContext _context;

        public BidsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Add(Bid bid)
        {
            _context.Bids.Add(bid);
            await _context.SaveChangesAsync();
        }

        public IQueryable<Bid> GetAll()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var applicationDbContext = from a in _context.Bids.Include(l => l.Listing).ThenInclude(static l => l.User)
                                       select a;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            return applicationDbContext;
        }
    }
}
