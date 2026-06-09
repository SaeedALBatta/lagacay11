using lagacay11.Data.Repositories;
using lagacay11.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace lagacay11.Controllers
{
    [Authorize]
    public class TestimonialsController : Controller
    {
        private readonly ITestimonialRepository _testimonialRepository;

        public TestimonialsController(ITestimonialRepository testimonialRepository)
        {
            _testimonialRepository = testimonialRepository;
        }

        // POST: /Testimonials/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Testimonial content cannot be empty.";
                return RedirectToAction("Index", "Home");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Challenge();
            }

            var testimonial = new Testimonial
            {
                UserId = userId,
                Content = content,
                IsApproved = false, // Requires admin moderation
                CreatedAt = DateTime.UtcNow
            };

            await _testimonialRepository.AddAsync(testimonial);

            TempData["SuccessMessage"] = "Thank you! Your testimonial has been submitted and is awaiting administrator approval.";
            return RedirectToAction("Index", "Home");
        }
    }
}
