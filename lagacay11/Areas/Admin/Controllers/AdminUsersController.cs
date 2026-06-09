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
    public class AdminUsersController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;

        public AdminUsersController(IUserRepository userRepository, IOrderRepository orderRepository)
        {
            _userRepository = userRepository;
            _orderRepository = orderRepository;
        }

        // GET: /Admin/AdminUsers
        public async Task<IActionResult> Index(string? search)
        {
            var users = await _userRepository.ListAllAsync(search);

            var model = new AdminUserListViewModel
            {
                Users = users.Select(u => new UserViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    PhoneNumber = u.PhoneNumber ?? string.Empty,
                    Address = u.Address ?? string.Empty,
                    CreatedAt = u.CreatedAt
                }).ToList(),
                SelectedSearch = search
            };

            return View(model);
        }

        // GET: /Admin/AdminUsers/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var orders = await _orderRepository.ListUserOrdersAsync(id);

            // Parse address
            string street = user.Address ?? string.Empty;
            string city = "—";
            string postalCode = "—";
            string country = "—";

            if (!string.IsNullOrEmpty(user.Address))
            {
                var parts = user.Address.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    street = parts[0].Trim();
                    city = parts[1].Trim();
                    country = parts[parts.Length - 1].Trim();
                    if (parts.Length > 3)
                    {
                        postalCode = parts[2].Trim();
                    }
                }
            }

            // Calculate KPIs
            int totalOrders = orders.Count;
            decimal totalSpent = orders.Sum(o => o.TotalAmount);
            decimal avgValue = totalOrders > 0 ? totalSpent / totalOrders : 0;
            DateTime? lastOrderDate = orders.FirstOrDefault()?.OrderDate;

            var model = new AdminUserDetailsViewModel
            {
                Id = user.Id,
                FullName = user.FullName ?? "Unknown Collector",
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Username = !string.IsNullOrEmpty(user.UserName) ? "@" + user.UserName.Split('@')[0] : "@unknown",
                Address = user.Address ?? string.Empty,
                ShippingStreet = street,
                ShippingCity = city,
                ShippingPostalCode = postalCode,
                ShippingCountry = country,
                CreatedAt = user.CreatedAt,
                IsActive = true,
                Role = user.Email == "admin@worldcupstore.com" ? "Administrator" : "Customer",
                TotalOrders = totalOrders,
                TotalSpent = totalSpent,
                AvgOrderValue = avgValue,
                LastOrderDate = lastOrderDate,
                RecentOrders = orders.Select(o => new OrderSummaryViewModel
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    PaymentReference = o.PaymentReference ?? string.Empty,
                    Status = o.Status,
                    CustomerName = user.FullName ?? "Unknown"
                }).ToList()
            };

            return View(model);
        }
    }
}
