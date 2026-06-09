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
    public class AdminTestimonialsController : Controller
    {
        private readonly ITestimonialRepository _testimonialRepository;

        public AdminTestimonialsController(ITestimonialRepository testimonialRepository)
        {
            _testimonialRepository = testimonialRepository;
        }

        // GET: /Admin/AdminTestimonials
        public async Task<IActionResult> Index()
        {
            var testimonials = await _testimonialRepository.ListAllAsync();

            var model = new AdminTestimonialListViewModel
            {
                Testimonials = testimonials.Select(t => new TestimonialViewModel
                {
                    Id = t.Id,
                    UserFullName = t.User?.FullName ?? "Unknown User",
                    UserEmail = t.User?.Email ?? string.Empty,
                    Content = t.Content,
                    IsApproved = t.IsApproved,
                    CreatedAt = t.CreatedAt
                }).ToList()
            };

            return View(model);
        }

        // POST: /Admin/AdminTestimonials/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var testimonial = await _testimonialRepository.GetByIdAsync(id);
            if (testimonial != null)
            {
                testimonial.IsApproved = true;
                await _testimonialRepository.UpdateAsync(testimonial);
                TempData["SuccessMessage"] = "Testimonial approved successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/AdminTestimonials/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var testimonial = await _testimonialRepository.GetByIdAsync(id);
            if (testimonial != null)
            {
                await _testimonialRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "Testimonial rejected and deleted.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
