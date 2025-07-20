using Auctions.Models;

namespace Auctions.Data.Services
{
    public interface ICommentsService
    {
        Task Add(Comment comment);
        Task Delete(int id);

        Task<Comment?> GetById(int id);

    }
}
