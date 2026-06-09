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
    public class AdminOrdersController : Controller
    {
        private readonly IOrderRepository _orderRepository;

        public AdminOrdersController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // GET: /Admin/AdminOrders
        public async Task<IActionResult> Index(string? status, DateTime? startDate, DateTime? endDate, string? search)
        {
            var orders = await _orderRepository.ListAdminOrdersAsync(status, startDate, endDate, search);

            var orderSummaries = orders.Select(o => new OrderSummaryViewModel
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                PaymentReference = o.PaymentReference ?? string.Empty,
                Status = o.Status,
                CustomerName = o.User?.FullName ?? "Unknown"
            }).ToList();

            var model = new AdminOrderListViewModel
            {
                Orders = orderSummaries,
                SelectedStatus = status,
                SelectedStartDate = startDate?.ToString("yyyy-MM-dd"),
                SelectedEndDate = endDate?.ToString("yyyy-MM-dd"),
                SelectedSearch = search
            };

            return View(model);
        }

        // GET: /Admin/AdminOrders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderRepository.GetByIdWithUserAndItemsAdminAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var model = new OrderDetailViewModel
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress ?? string.Empty,
                PaymentReference = order.PaymentReference ?? string.Empty,
                CustomerName = order.User?.FullName ?? "Unknown",
                CustomerEmail = order.User?.Email ?? string.Empty,
                CustomerPhone = order.User?.PhoneNumber ?? string.Empty,
                OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "Unknown Product",
                    ProductYear = oi.Product?.Year ?? 0,
                    WorldCupEdition = oi.Product?.WorldCupEdition ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Subtotal = oi.UnitPrice * oi.Quantity
                }).ToList()
            };

            return View(model);
        }

        // POST: /Admin/AdminOrders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            if (status == "Processing" || status == "Completed" || status == "Cancelled")
            {
                order.Status = status;
                await _orderRepository.UpdateAsync(order);
                TempData["SuccessMessage"] = $"Order #{orderId} status updated to {status}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid status selected.";
            }

            return RedirectToAction(nameof(Details), new { id = orderId });
        }
    }
}
