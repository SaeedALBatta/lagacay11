using lagacay11.Data.Repositories;
using lagacay11.Models;
using lagacay11.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace lagacay11.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IOrderRepository _orderRepository;

        public ProductsController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IReviewRepository reviewRepository,
            IOrderRepository orderRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _reviewRepository = reviewRepository;
            _orderRepository = orderRepository;
        }

        // GET: /Products
        public async Task<IActionResult> Index(
            string? search, 
            string? category, 
            string? team, 
            string? club, 
            int? year, 
            string? edition, 
            string? sort)
        {
            var products = await _productRepository.GetAvailableProductsAsync(search, category, team, club, year, edition, sort);
            var categories = await _categoryRepository.ListAllAsync();
            
            // Build dropdown options
            var wcEditions = products
                .Where(p => !string.IsNullOrEmpty(p.WorldCupEdition))
                .Select(p => p.WorldCupEdition)
                .Distinct()
                .ToList();
            
            var years = products
                .Select(p => p.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            var viewModel = new ProductListViewModel
            {
                Products = products.Select(MapToProductViewModel).ToList(),
                Categories = categories.Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description
                }).ToList(),
                WorldCupEditions = wcEditions,
                Years = years,
                SelectedSearch = search,
                SelectedCategory = category,
                SelectedTeam = team,
                SelectedClub = club,
                SelectedYear = year,
                SelectedEdition = edition,
                SelectedSort = sort
            };

            return View(viewModel);
        }

        // GET: /Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdWithDetailsAsync(id);
            if (product == null || !product.IsAvailable)
            {
                return NotFound();
            }

            var approvedReviews = product.Reviews
                .Where(r => r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    UserFullName = r.User?.FullName ?? "Anonymous",
                    Comment = r.Comment,
                    Rating = r.Rating,
                    IsApproved = r.IsApproved,
                    CreatedAt = r.CreatedAt
                }).ToList();

            var averageRating = approvedReviews.Any() ? approvedReviews.Average(r => r.Rating) : 0.0;
            var categoryIds = product.ProductCategories.Select(pc => pc.CategoryId).ToList();
            
            var relatedProducts = await _productRepository.GetRelatedProductsAsync(id, categoryIds, 4);

            var canUserReview = false;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                canUserReview = await _orderRepository.HasUserPurchasedProductAsync(userId, id);
            }

            var viewModel = new ProductDetailsViewModel
            {
                Product = MapToProductViewModel(product),
                ApprovedReviews = approvedReviews,
                AverageRating = averageRating,
                RelatedProducts = relatedProducts.Select(MapToProductViewModel).ToList(),
                CanUserReview = canUserReview
            };

            return View(viewModel);
        }

        // POST: /Products/SubmitReview
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(int productId, string comment, int rating)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["ErrorMessage"] = "Review comment cannot be empty.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }

            if (rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] = "Rating must be between 1 and 5 stars.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Challenge();
            }

            var hasPurchased = await _orderRepository.HasUserPurchasedProductAsync(userId, productId);
            if (!hasPurchased)
            {
                TempData["ErrorMessage"] = "You must purchase this product before writing a review.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }

            var review = new Review
            {
                ProductId = productId,
                UserId = userId,
                Comment = comment,
                Rating = rating,
                IsApproved = false, // Defaults to unapproved
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);

            TempData["SuccessMessage"] = "Thank you! Your product review has been submitted and is awaiting administrator approval.";
            return RedirectToAction(nameof(Details), new { id = productId });
        }

        private ProductViewModel MapToProductViewModel(Product p)
        {
            return new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                TeamName = p.TeamName,
                ClubName = p.ClubName,
                Year = p.Year,
                WorldCupEdition = p.WorldCupEdition,
                IsAvailable = p.IsAvailable,
                MainImagePath = p.ProductImages.FirstOrDefault(pi => pi.IsMain)?.ImagePath ?? p.ProductImages.FirstOrDefault()?.ImagePath ?? "/uploads/products/placeholder.jpg",
                ImagePaths = p.ProductImages.OrderBy(pi => pi.SortOrder).Select(pi => pi.ImagePath).ToList(),
                Categories = p.ProductCategories.Select(pc => new CategoryViewModel
                {
                    Id = pc.Category.Id,
                    Name = pc.Category.Name,
                    Slug = pc.Category.Slug
                }).ToList()
            };
        }
    }
}
