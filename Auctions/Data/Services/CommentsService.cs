using Auctions.Models;
using Microsoft.EntityFrameworkCore;

namespace Auctions.Data.Services
{
    public class CommentsService(ApplicationDbContext context) : ICommentsService
    {
        private readonly ApplicationDbContext _context = context;

        public async Task Add(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Comment?> GetById(int id)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Listing)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public Task<string?> GetById(object value)
        {
            throw new NotImplementedException();
        }
    }
}
