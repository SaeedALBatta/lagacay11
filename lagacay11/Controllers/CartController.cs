using lagacay11.Data;
using lagacay11.Data.Repositories;
using lagacay11.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lagacay11.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICouponRepository _couponRepository;
        private const string CartSessionKey = "SessionCart";
        private const string CouponSessionKey = "AppliedCoupon";

        public CartController(IProductRepository productRepository, ICouponRepository couponRepository)
        {
            _productRepository = productRepository;
            _couponRepository = couponRepository;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var cart = GetCart();
            var total = cart.Sum(item => item.Subtotal);

            var viewModel = new CartViewModel
            {
                Items = cart,
                Total = total,
                DiscountAmount = 0,
                FinalTotal = total
            };

            string? appliedCoupon = HttpContext.Session.GetString(CouponSessionKey);
            if (!string.IsNullOrEmpty(appliedCoupon) && cart.Any())
            {
                var (discount, error) = await CalculateDiscountAsync(appliedCoupon, total);
                if (error == null)
                {
                    viewModel.CouponCode = appliedCoupon;
                    viewModel.DiscountAmount = discount;
                    viewModel.FinalTotal = total - discount;
                }
                else
                {
                    // Remove stale/invalid coupon from session
                    HttpContext.Session.Remove(CouponSessionKey);
                }
            }

            return View(viewModel);
        }

        // POST: /Cart/Add
        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var product = await _productRepository.GetByIdWithDetailsAsync(productId);

            if (product == null || !product.IsAvailable || product.StockQuantity <= 0)
            {
                TempData["ErrorMessage"] = "Product is not available or out of stock.";
                return RedirectToAction(nameof(Index));
            }

            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(i => i.ProductId == productId);

            int requestedQuantity = (cartItem?.Quantity ?? 0) + quantity;

            if (requestedQuantity > product.StockQuantity)
            {
                TempData["ErrorMessage"] = $"Cannot add {quantity} more item(s). Only {product.StockQuantity} in stock.";
                return RedirectToAction(nameof(Index));
            }

            if (cartItem == null)
            {
                var mainImage = product.ProductImages.FirstOrDefault(i => i.IsMain)?.ImagePath 
                                ?? product.ProductImages.FirstOrDefault()?.ImagePath 
                                ?? "/uploads/products/placeholder.jpg";

                cart.Add(new CartItemViewModel
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImagePath = mainImage
                });
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            SaveCart(cart);
            TempData["SuccessMessage"] = $"Added {product.Name} to your cart.";
            
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(i => i.ProductId == productId);

            if (cartItem != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(cartItem);
                    TempData["SuccessMessage"] = $"Removed {product.Name} from your cart.";
                }
                else if (quantity > product.StockQuantity)
                {
                    TempData["ErrorMessage"] = $"Cannot set quantity to {quantity}. Only {product.StockQuantity} items available in stock.";
                }
                else
                {
                    cartItem.Quantity = quantity;
                    TempData["SuccessMessage"] = $"Updated quantity for {product.Name}.";
                }
                SaveCart(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Remove
        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(i => i.ProductId == productId);

            if (cartItem != null)
            {
                cart.Remove(cartItem);
                SaveCart(cart);
                TempData["SuccessMessage"] = $"Removed {cartItem.Name} from your cart.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Clear
        [HttpPost]
        public IActionResult Clear()
        {
            SaveCart(new List<CartItemViewModel>());
            TempData["SuccessMessage"] = "Cleared your cart.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/ApplyCoupon
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string couponCode)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
            {
                TempData["ErrorMessage"] = "Please enter a coupon code.";
                return RedirectToAction(nameof(Index));
            }

            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Cannot apply coupon to an empty cart.";
                return RedirectToAction(nameof(Index));
            }

            decimal subtotal = cart.Sum(i => i.Subtotal);
            var (discount, error) = await CalculateDiscountAsync(couponCode, subtotal);

            if (error != null)
            {
                TempData["ErrorMessage"] = error;
                HttpContext.Session.Remove(CouponSessionKey);
            }
            else
            {
                HttpContext.Session.SetString(CouponSessionKey, couponCode.Trim().ToUpper());
                TempData["SuccessMessage"] = $"Coupon '{couponCode.Trim().ToUpper()}' applied successfully! Saved ${discount:F2}.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/RemoveCoupon
        [HttpPost]
        public IActionResult RemoveCoupon()
        {
            HttpContext.Session.Remove(CouponSessionKey);
            TempData["SuccessMessage"] = "Coupon removed successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<(decimal discountAmount, string? error)> CalculateDiscountAsync(string couponCode, decimal cartSubtotal)
        {
            var coupon = await _couponRepository.GetByCodeAsync(couponCode.Trim().ToUpper());
            if (coupon == null)
            {
                return (0, "Coupon code not found.");
            }

            if (!coupon.IsActive)
            {
                return (0, "This coupon has been paused.");
            }

            if (coupon.ExpirationDate.HasValue && coupon.ExpirationDate.Value < DateTime.UtcNow)
            {
                return (0, "This coupon has expired.");
            }

            if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
            {
                return (0, "This coupon has reached its usage limit.");
            }

            if (coupon.MinimumPurchase.HasValue && cartSubtotal < coupon.MinimumPurchase.Value)
            {
                return (0, $"Minimum purchase of ${coupon.MinimumPurchase.Value:F2} is required to use this coupon.");
            }

            decimal discount = 0;
            if (coupon.Type.Equals("fixed", StringComparison.OrdinalIgnoreCase))
            {
                discount = coupon.Value;
            }
            else // percentage
            {
                discount = cartSubtotal * (coupon.Value / 100m);
            }

            // Ensure discount doesn't exceed subtotal
            if (discount > cartSubtotal)
            {
                discount = cartSubtotal;
            }

            return (discount, null);
        }

        private List<CartItemViewModel> GetCart()
        {
            return HttpContext.Session.Get<List<CartItemViewModel>>(CartSessionKey) ?? new List<CartItemViewModel>();
        }

        private void SaveCart(List<CartItemViewModel> cart)
        {
            HttpContext.Session.Set(CartSessionKey, cart);
        }
    }
}
