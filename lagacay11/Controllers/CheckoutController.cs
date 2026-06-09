using lagacay11.Data;
using lagacay11.Data.Repositories;
using lagacay11.Models;
using lagacay11.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace lagacay11.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICouponRepository _couponRepository;
        private const string CartSessionKey = "SessionCart";
        private const string ShippingAddressSessionKey = "CheckoutShippingAddress";

        public CheckoutController(
            IProductRepository productRepository, 
            IOrderRepository orderRepository,
            UserManager<ApplicationUser> userManager,
            ICouponRepository couponRepository)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _userManager = userManager;
            _couponRepository = couponRepository;
        }

        // GET: /Checkout
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty. Please add items before checking out.";
                return RedirectToAction("Index", "Cart");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Challenge();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Challenge();
            }

            decimal subtotal = cart.Sum(item => item.Subtotal);
            decimal discount = 0;
            string? appliedCoupon = HttpContext.Session.GetString("AppliedCoupon");

            if (!string.IsNullOrEmpty(appliedCoupon))
            {
                var (disc, error) = await CalculateDiscountAsync(appliedCoupon, subtotal);
                if (error == null)
                {
                    discount = disc;
                }
                else
                {
                    HttpContext.Session.Remove("AppliedCoupon");
                    appliedCoupon = null;
                }
            }

            var viewModel = new CheckoutPageViewModel
            {
                Form = new CheckoutViewModel
                {
                    ShippingAddress = user.Address
                },
                CartItems = cart,
                CartTotal = subtotal,
                CouponCode = appliedCoupon,
                DiscountAmount = discount,
                FinalTotal = subtotal - discount
            };

            return View(viewModel);
        }

        // POST: /Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutPageViewModel wrapper)
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty. Please add items before checking out.";
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                wrapper.CartItems = cart;
                wrapper.CartTotal = cart.Sum(item => item.Subtotal);
                return View("Index", wrapper);
            }

            // 1. Check stock levels
            foreach (var item in cart)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null || !product.IsAvailable)
                {
                    TempData["ErrorMessage"] = $"Product '{item.Name}' is no longer available.";
                    return RedirectToAction("Index", "Cart");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    TempData["ErrorMessage"] = $"Insufficient stock for '{product.Name}'. Only {product.StockQuantity} items remaining.";
                    return RedirectToAction("Index", "Cart");
                }
            }

            // Save shipping address to session
            HttpContext.Session.SetString(ShippingAddressSessionKey, wrapper.Form.ShippingAddress);

            // Calculate discount factor if applied
            decimal subtotal = cart.Sum(item => item.Subtotal);
            decimal discount = 0;
            string? appliedCoupon = HttpContext.Session.GetString("AppliedCoupon");
            if (!string.IsNullOrEmpty(appliedCoupon))
            {
                var (disc, error) = await CalculateDiscountAsync(appliedCoupon, subtotal);
                if (error == null)
                {
                    discount = disc;
                }
            }
            decimal finalTotal = subtotal - discount;
            decimal scaleFactor = subtotal > 0 ? finalTotal / subtotal : 1.0m;

            // 2. Build Stripe session
            var domain = $"{Request.Scheme}://{Request.Host}";
            var lineItems = new List<SessionLineItemOptions>();

            foreach (var item in cart)
            {
                decimal itemDiscountedPrice = item.Price * scaleFactor;
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)Math.Round(itemDiscountedPrice * 100, MidpointRounding.AwayFromZero),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Name,
                            Description = "Vintage Football Jersey"
                        }
                    },
                    Quantity = item.Quantity
                });
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = $"{domain}/Checkout/PaymentSuccess?sessionId={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/Checkout/Index"
            };

            var service = new SessionService();
            try
            {
                var stripeSession = await service.CreateAsync(options);
                return Redirect(stripeSession.Url);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Stripe payment service error: {ex.Message}");
                wrapper.CartItems = cart;
                wrapper.CartTotal = cart.Sum(item => item.Subtotal);
                return View("Index", wrapper);
            }
        }

        // GET: /Checkout/PaymentSuccess
        [HttpGet]
        public async Task<IActionResult> PaymentSuccess(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                TempData["ErrorMessage"] = "Payment session reference is missing.";
                return RedirectToAction("Index", "Cart");
            }

            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Your cart session ended or is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Challenge();
            }

            var service = new SessionService();
            Session stripeSession;
            try
            {
                stripeSession = await service.GetAsync(sessionId);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Failed to verify transaction details with Stripe.";
                return RedirectToAction("Index", "Cart");
            }

            if (stripeSession.PaymentStatus != "paid")
            {
                TempData["ErrorMessage"] = "Payment was not completed successfully.";
                return RedirectToAction("Index", "Cart");
            }

            var shippingAddress = HttpContext.Session.GetString(ShippingAddressSessionKey);
            if (string.IsNullOrEmpty(shippingAddress))
            {
                var user = await _userManager.FindByIdAsync(userId);
                shippingAddress = user?.Address ?? "No address registered";
            }

            // Create Order records (Repositories update products and add orders)
            try
            {
                var orderItems = new List<OrderItem>();
                decimal subtotal = 0;

                foreach (var item in cart)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product == null || !product.IsAvailable || product.StockQuantity < item.Quantity)
                    {
                        TempData["ErrorMessage"] = $"Transaction failed: Item '{item.Name}' ran out of stock during payment processing.";
                        return RedirectToAction("Index", "Cart");
                    }

                    // Decrement stock
                    product.StockQuantity -= item.Quantity;
                    await _productRepository.UpdateAsync(product);

                    orderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price,
                        Subtotal = item.Subtotal
                    });

                    subtotal += item.Subtotal;
                }

                decimal discount = 0;
                var appliedCoupon = HttpContext.Session.GetString("AppliedCoupon");
                if (!string.IsNullOrEmpty(appliedCoupon))
                {
                    var (disc, error) = await CalculateDiscountAsync(appliedCoupon, subtotal);
                    if (error == null)
                    {
                        discount = disc;

                        // Increment usage count of the coupon
                        var coupon = await _couponRepository.GetByCodeAsync(appliedCoupon);
                        if (coupon != null)
                        {
                            coupon.UsageCount++;
                            await _couponRepository.UpdateAsync(coupon);
                        }
                    }
                }

                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = subtotal - discount,
                    Status = "Processing",
                    ShippingAddress = shippingAddress,
                    PaymentReference = stripeSession.PaymentIntentId ?? stripeSession.Id,
                    OrderItems = orderItems
                };

                await _orderRepository.AddAsync(order);

                // Clear sessions
                SaveCart(new List<CartItemViewModel>());
                HttpContext.Session.Remove(ShippingAddressSessionKey);
                HttpContext.Session.Remove("AppliedCoupon");

                return RedirectToAction(nameof(Confirmation), new { id = order.Id });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while saving your order details in our database.";
                return RedirectToAction("Index", "Cart");
            }
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

        // GET: /Checkout/Confirmation
        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Challenge();
            }

            var order = await _orderRepository.GetByIdWithUserAndItemsAsync(id, userId);
            if (order == null)
            {
                return NotFound();
            }

            var viewModel = new OrderDetailViewModel
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                PaymentReference = order.PaymentReference,
                CustomerName = order.User?.FullName ?? "Customer",
                OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "Vintage Jersey",
                    ProductYear = oi.Product?.Year ?? 0,
                    WorldCupEdition = oi.Product?.WorldCupEdition ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Subtotal = oi.Subtotal
                }).ToList()
            };

            return View(viewModel);
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
