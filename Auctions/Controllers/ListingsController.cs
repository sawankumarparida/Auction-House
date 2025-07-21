using System.Security.Claims;
using Auctions.Data.Services;
using Auctions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Auctions.Controllers
{
    public class ListingsController(IListingsService listingsService, IWebHostEnvironment webHostEnvironment, IBidsService bidsService, ICommentsService commentsService) : Controller
    {
        private readonly IListingsService _listingsService = listingsService;
        private readonly IBidsService _bidsService = bidsService;
        private readonly ICommentsService _commentsService = commentsService;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        // Removed unused _context field

        // GET: Listings
        public async Task<IActionResult> Index(int? pageNumber, string searchString)
        {
            var applicationDbContext = _listingsService.GetAll();
            int pageSize = 3;
            if (!string.IsNullOrEmpty(searchString))
            {
                applicationDbContext = applicationDbContext.Where(a => a.Title.Contains(searchString));
                return View(await PaginatedList<Listing>.CreateAsync(applicationDbContext.Where(l => l.IsSold == false).AsNoTracking(), pageNumber ?? 1, pageSize));

            }

            return View(await PaginatedList<Listing>.CreateAsync(applicationDbContext.Where(l => l.IsSold == false).AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        //private IActionResult View(object value)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<IActionResult> MyListings(int? pageNumber)
        {
            var applicationDbContext = _listingsService.GetAll();
            int pageSize = 3;

            return View("Index", await PaginatedList<Listing>.CreateAsync(applicationDbContext.Where(l => l.IdentityUserId == User.FindFirstValue(ClaimTypes.NameIdentifier)).AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        public async Task<IActionResult> MyBids(int? pageNumber)
        {
            var applicationDbContext = _bidsService.GetAll();
            int pageSize = 3;

            return View(await PaginatedList<Bid>.CreateAsync(applicationDbContext.Where(l => l.IdentityUserId == User.FindFirstValue(ClaimTypes.NameIdentifier)).AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Listings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listing = await _listingsService.GetById(id);

            if (listing == null)
            {
                return NotFound();
            }

            return View(listing);
        }

        // GET: Listings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Listings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ListingVM listing)
        {
            if (listing.Image != null)
            {
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
                string fileName = listing.Image.FileName;
                string filePath = Path.Combine(uploadDir, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    listing.Image.CopyTo(fileStream);
                }

                var listObj = new Listing
                {
                    Title = listing.Title,
                    Description = listing.Description,
                    Price = listing.Price,
                    IdentityUserId = listing.IdentityUserId,
                    ImagePath = fileName,
                };
                await _listingsService.Add(listObj);
                return RedirectToAction("Index");
            }
            return View(listing);
        }
        [HttpPost]
        public async Task<ActionResult> AddBid([Bind("Id, Price, ListingId, IdentityUserId")] Bid bid)
        {
            if (ModelState.IsValid)
            {
                await _bidsService.Add(bid);
            }
            var listing = await _listingsService.GetById(bid.ListingId);
            listing.Price = bid.Price;
            await _listingsService.SaveChanges();

            return View("Details", listing);
        }
        public async Task<ActionResult> CloseBidding(int id)
        {
            var listing = await _listingsService.GetById(id);
            listing.IsSold = true;
            await _listingsService.SaveChanges();
            return View("Details", listing);
        }
        // GET: Comments/Create
#pragma warning disable ASP0023 // Route conflict detected between controller actions
        [HttpPost("{id}"), ActionName("AddComment")]
#pragma warning restore ASP0023 // Route conflict detected between controller actions
        public async Task<ActionResult> AddComment(string id, [Bind("Id, Content, ListingId, IdentityUserId")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                await _commentsService.Add(comment);
            }
            var listing = await _listingsService.GetById(comment.ListingId);
            return View("Details", listing);
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> DeleteComment(int id)
        {
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            if (id == null)
                return NotFound();
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'

            var comment = await _commentsService.GetById(id); // id is already int, no need for .Value
            if (comment == null)
                return NotFound();

            return View(comment); // Show confirmation page
        }

        // POST: Comments/Delete/5
#pragma warning disable ASP0023 // Route conflict detected between controller actions
        [HttpPost("{id}"), ActionName("DeleteComment")]
#pragma warning restore ASP0023 // Route conflict detected between controller actions
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCommentConfirmed(int id)
        {
            await _commentsService.Delete(id);
            return RedirectToAction("Index", "Listings"); // Or wherever you want to redirect
        }

        //
        // GET: Listings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listing = await _listingsService.GetById(id);
            if (listing == null)
            {
                return NotFound();
            }
            // If you need to populate ViewData["IdentityUserId"], you may need to fetch users from a user service or context.
            // ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", listing.IdentityUserId);
            return View(listing);
        }

        // POST: Listings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Price,ImagePath,IsSold,IdentityUserId")] Listing listing)
        {
            if (id != listing.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _listingsService.Update(listing);
                    await _listingsService.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListingExists(listing.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            // ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", listing.IdentityUserId);
            return View(listing);
        }



        // GET: Listings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listing = await _listingsService.GetAll()
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (listing == null)
            {
                return NotFound();
            }

            return View(listing);
        }

        // POST: Listings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var listing = await _listingsService.GetById(id);
            if (listing != null)
            {
                await _listingsService.Delete(listing);
                await _listingsService.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ListingExists(int id)
        {
            var listing = _listingsService.GetById(id).Result;
            return listing != null;
        }
    }
}
