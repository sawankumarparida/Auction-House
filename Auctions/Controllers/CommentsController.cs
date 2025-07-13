using Microsoft.AspNetCore.Mvc;
using Auctions.Data.Services; // Make sure this matches your namespace for ICommentsService

public class CommentsController : Controller
{
    private readonly ICommentsService _commentsService;

    public CommentsController(ICommentsService commentsService)
    {
        _commentsService = commentsService;
    }

    // GET: Comments/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var comment = await _commentsService.GetById(id.Value); // Use id.Value to avoid nullable issues
        if (comment == null)
            return NotFound();

        return View(comment); // Show confirmation page
    }

    // POST: Comments/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _commentsService.Delete(id);
        return RedirectToAction("Index", "Listings"); // Or wherever you want to redirect
    }
}