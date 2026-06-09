using lagacay11.Data.Repositories;
using lagacay11.Models;
using lagacay11.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace lagacay11.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminReviewsController : Controller
    {
        private readonly IReviewRepository _reviewRepository;

        public AdminReviewsController(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        // GET: /Admin/AdminReviews
        public async Task<IActionResult> Index()
        {
            var reviews = await _reviewRepository.ListAllAsync();

            var model = new AdminReviewListViewModel
            {
                Reviews = reviews.Select(r => new AdminReviewViewModel
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    ProductName = r.Product?.Name ?? "Unknown Product",
                    CustomerName = r.User?.FullName ?? "Unknown Customer",
                    Comment = r.Comment,
                    Rating = r.Rating,
                    IsApproved = r.IsApproved,
                    CreatedAt = r.CreatedAt
                }).ToList()
            };

            return View(model);
        }

        // POST: /Admin/AdminReviews/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review != null)
            {
                review.IsApproved = true;
                await _reviewRepository.UpdateAsync(review);
                TempData["SuccessMessage"] = "Review approved successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/AdminReviews/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review != null)
            {
                await _reviewRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "Review rejected and deleted.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
