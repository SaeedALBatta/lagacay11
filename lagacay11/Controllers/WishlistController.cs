using lagacay11.Data.Repositories;
using lagacay11.Models;
using lagacay11.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace lagacay11.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IProductRepository _productRepository;

        public WishlistController(IWishlistRepository wishlistRepository, IProductRepository productRepository)
        {
            _wishlistRepository = wishlistRepository;
            _productRepository = productRepository;
        }

        // GET: /Wishlist
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Challenge();
            }

            var wishlistItems = await _wishlistRepository.ListUserWishlistAsync(userId);
            
            var viewModel = new WishlistViewModel
            {
                Products = wishlistItems.Select(w => new ProductViewModel
                {
                    Id = w.Product.Id,
                    Name = w.Product.Name,
                    Description = w.Product.Description,
                    Price = w.Product.Price,
                    StockQuantity = w.Product.StockQuantity,
                    TeamName = w.Product.TeamName,
                    ClubName = w.Product.ClubName,
                    Year = w.Product.Year,
                    WorldCupEdition = w.Product.WorldCupEdition,
                    IsAvailable = w.Product.IsAvailable,
                    MainImagePath = w.Product.ProductImages.FirstOrDefault(pi => pi.IsMain)?.ImagePath 
                                    ?? w.Product.ProductImages.FirstOrDefault()?.ImagePath 
                                    ?? "/uploads/products/placeholder.jpg",
                    ImagePaths = w.Product.ProductImages.OrderBy(pi => pi.SortOrder).Select(pi => pi.ImagePath).ToList()
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: /Wishlist/Add
        [HttpPost]
        public async Task<IActionResult> Add(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Challenge();
            }

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            // Check if already in wishlist
            var alreadyInWishlist = await _wishlistRepository.AnyAsync(userId, productId);

            if (!alreadyInWishlist)
            {
                var wishlistItem = new Wishlist
                {
                    UserId = userId,
                    ProductId = productId,
                    AddedAt = DateTime.UtcNow
                };

                await _wishlistRepository.AddAsync(wishlistItem);
                TempData["SuccessMessage"] = $"Added {product.Name} to your wishlist.";
            }
            else
            {
                TempData["ErrorMessage"] = $"{product.Name} is already in your wishlist.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Wishlist/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Challenge();
            }

            var exists = await _wishlistRepository.AnyAsync(userId, productId);
            if (exists)
            {
                await _wishlistRepository.RemoveAsync(userId, productId);
                TempData["SuccessMessage"] = "Removed item from your wishlist.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
