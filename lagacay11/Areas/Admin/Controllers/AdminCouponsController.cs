using lagacay11.Data.Repositories;
using lagacay11.Models;
using lagacay11.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace lagacay11.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminCouponsController : Controller
    {
        private readonly ICouponRepository _couponRepository;

        public AdminCouponsController(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
        }

        // GET: /Admin/AdminCoupons
        public async Task<IActionResult> Index(string? search, string? status)
        {
            var coupons = await _couponRepository.ListAllAsync(search);
            
            // Calculate stats before status filtering
            var activeCouponsCount = coupons.Count(c => c.IsActive && (c.ExpirationDate == null || c.ExpirationDate >= DateTime.UtcNow));
            var totalUsesCount = coupons.Sum(c => c.UsageCount);
            var totalDiscountedValue = coupons.Sum(c => c.Type == "fixed" ? c.Value * c.UsageCount : c.UsageCount * 25.50m);

            // Filter by status if selected
            if (!string.IsNullOrEmpty(status) && status != "All Statuses")
            {
                var now = DateTime.UtcNow;
                if (status == "Active")
                {
                    coupons = coupons.Where(c => c.IsActive && (c.ExpirationDate == null || c.ExpirationDate >= now)).ToList();
                }
                else if (status == "Expired")
                {
                    coupons = coupons.Where(c => c.ExpirationDate < now).ToList();
                }
                else if (status == "Paused")
                {
                    coupons = coupons.Where(c => !c.IsActive).ToList();
                }
            }

            var viewModel = new AdminCouponListViewModel
            {
                Coupons = coupons,
                SelectedSearch = search,
                SelectedStatus = status,
                ActiveCouponsCount = activeCouponsCount,
                TotalUsesCount = totalUsesCount,
                TotalDiscountedValue = totalDiscountedValue
            };

            return View(viewModel);
        }

        // GET: /Admin/AdminCoupons/Create
        public IActionResult Create()
        {
            return View(new AdminCouponViewModel());
        }

        // POST: /Admin/AdminCoupons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminCouponViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if code already exists
                var existing = await _couponRepository.GetByCodeAsync(model.Code.Trim().ToUpper());
                if (existing != null)
                {
                    ModelState.AddModelError("Code", "A coupon with this code already exists.");
                    return View(model);
                }

                var coupon = new Coupon
                {
                    Code = model.Code.Trim().ToUpper(),
                    Description = model.Description ?? string.Empty,
                    Type = model.Type,
                    Value = model.Value,
                    ExpirationDate = model.ExpirationDate,
                    UsageLimit = model.UsageLimit,
                    UsageCount = 0,
                    MinimumPurchase = model.MinimumPurchase,
                    IsActive = model.IsActive
                };

                await _couponRepository.AddAsync(coupon);
                TempData["SuccessMessage"] = $"Coupon '{coupon.Code}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /Admin/AdminCoupons/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }

            var model = new AdminCouponViewModel
            {
                Id = coupon.Id,
                Code = coupon.Code,
                Description = coupon.Description,
                Type = coupon.Type,
                Value = coupon.Value,
                ExpirationDate = coupon.ExpirationDate,
                UsageLimit = coupon.UsageLimit,
                UsageCount = coupon.UsageCount,
                MinimumPurchase = coupon.MinimumPurchase,
                IsActive = coupon.IsActive
            };

            return View(model);
        }

        // POST: /Admin/AdminCoupons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminCouponViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Check if code already exists on another coupon
                var existing = await _couponRepository.GetByCodeAsync(model.Code.Trim().ToUpper());
                if (existing != null && existing.Id != model.Id)
                {
                    ModelState.AddModelError("Code", "A coupon with this code already exists.");
                    return View(model);
                }

                var coupon = await _couponRepository.GetByIdAsync(id);
                if (coupon == null)
                {
                    return NotFound();
                }

                coupon.Code = model.Code.Trim().ToUpper();
                coupon.Description = model.Description ?? string.Empty;
                coupon.Type = model.Type;
                coupon.Value = model.Value;
                coupon.ExpirationDate = model.ExpirationDate;
                coupon.UsageLimit = model.UsageLimit;
                coupon.MinimumPurchase = model.MinimumPurchase;
                coupon.IsActive = model.IsActive;

                await _couponRepository.UpdateAsync(coupon);
                TempData["SuccessMessage"] = $"Coupon '{coupon.Code}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: /Admin/AdminCoupons/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }

            await _couponRepository.DeleteAsync(id);
            TempData["SuccessMessage"] = $"Coupon '{coupon.Code}' deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
