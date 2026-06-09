using lagacay11.Data.Repositories;
using lagacay11.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace lagacay11.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserRepository _userRepository;

        public AdminDashboardController(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IUserRepository userRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
        }

        // GET: /Admin
        // GET: /Admin/AdminDashboard
        public async Task<IActionResult> Index()
        {
            var totalRevenue = await _orderRepository.GetTotalRevenueAsync();
            var totalOrders = await _orderRepository.GetCountAsync();
            var totalProducts = await _productRepository.GetCountAsync();
            var totalCategories = await _categoryRepository.GetCountAsync();
            var totalUsers = await _userRepository.GetCountAsync();

            var recentOrders = await _orderRepository.ListAdminOrdersAsync(null, null, null, null);
            var recentOrdersSummary = recentOrders.Take(5).Select(o => new OrderSummaryViewModel
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                PaymentReference = o.PaymentReference,
                Status = o.Status,
                CustomerName = o.User?.FullName ?? "Customer"
            }).ToList();

            // Calculate sales trend for the last 7 days
            var salesTrendLabels = new List<string>();
            var salesTrendData = new List<decimal>();
            var today = System.DateTime.UtcNow.Date;
            
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                salesTrendLabels.Add(date.ToString("ddd")); // e.g. "Mon", "Tue"
                
                var dailyRevenue = recentOrders
                    .Where(o => o.OrderDate.Date == date && o.Status != "Cancelled")
                    .Sum(o => o.TotalAmount);
                salesTrendData.Add(dailyRevenue);
            }

            // Order status counts
            var completedOrdersCount = recentOrders.Count(o => o.Status == "Completed");
            var pendingOrdersCount = recentOrders.Count(o => o.Status == "Processing" || o.Status == "Pending");
            var cancelledOrdersCount = recentOrders.Count(o => o.Status == "Cancelled");

            var viewModel = new AdminDashboardViewModel
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalProducts = totalProducts,
                TotalCategories = totalCategories,
                TotalUsers = totalUsers,
                RecentOrders = recentOrdersSummary,
                SalesTrendLabels = salesTrendLabels,
                SalesTrendData = salesTrendData,
                CompletedOrdersCount = completedOrdersCount,
                PendingOrdersCount = pendingOrdersCount,
                CancelledOrdersCount = cancelledOrdersCount
            };

            return View(viewModel);
        }
    }
}
