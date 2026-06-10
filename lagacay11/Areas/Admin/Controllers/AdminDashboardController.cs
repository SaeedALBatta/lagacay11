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
        public async Task<IActionResult> Index(string? period)
        {
            // Default period is 7 days
            int days = 7;
            if (period == "30") days = 30;
            else if (period == "365") days = 365;
            ViewBag.Period = days.ToString();

            var today = System.DateTime.UtcNow.Date;
            var periodStart = today.AddDays(-(days - 1));

            // All orders (for counts and chart)
            var allOrders = await _orderRepository.ListAdminOrdersAsync(null, null, null, null);

            // Orders within the selected period
            var periodOrders = allOrders
                .Where(o => o.OrderDate.Date >= periodStart && o.OrderDate.Date <= today)
                .ToList();

            var totalRevenue = periodOrders.Where(o => o.Status != "Cancelled").Sum(o => o.TotalAmount);
            var totalOrders = await _orderRepository.GetCountAsync();
            var totalProducts = await _productRepository.GetCountAsync();
            var totalCategories = await _categoryRepository.GetCountAsync();
            var totalUsers = await _userRepository.GetCountAsync();

            var recentOrdersSummary = periodOrders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new OrderSummaryViewModel
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    PaymentReference = o.PaymentReference,
                    Status = o.Status,
                    CustomerName = o.User?.FullName ?? "Customer"
                }).ToList();

            // Build sales trend labels/data across the selected period
            var salesTrendLabels = new List<string>();
            var salesTrendData = new List<decimal>();

            if (days <= 7)
            {
                // Daily for last 7 days
                for (int i = days - 1; i >= 0; i--)
                {
                    var date = today.AddDays(-i);
                    salesTrendLabels.Add(date.ToString("ddd"));
                    salesTrendData.Add(periodOrders
                        .Where(o => o.OrderDate.Date == date && o.Status != "Cancelled")
                        .Sum(o => o.TotalAmount));
                }
            }
            else if (days <= 30)
            {
                // Daily for last 30 days (group by week for readability)
                for (int i = days - 1; i >= 0; i--)
                {
                    var date = today.AddDays(-i);
                    salesTrendLabels.Add(date.ToString("MMM dd"));
                    salesTrendData.Add(periodOrders
                        .Where(o => o.OrderDate.Date == date && o.Status != "Cancelled")
                        .Sum(o => o.TotalAmount));
                }
            }
            else
            {
                // Monthly for the year
                for (int m = 11; m >= 0; m--)
                {
                    var monthStart = new System.DateTime(today.Year, today.Month, 1).AddMonths(-m);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    salesTrendLabels.Add(monthStart.ToString("MMM"));
                    salesTrendData.Add(periodOrders
                        .Where(o => o.OrderDate.Date >= monthStart && o.OrderDate.Date <= monthEnd && o.Status != "Cancelled")
                        .Sum(o => o.TotalAmount));
                }
            }

            // Order status counts within the period
            var completedOrdersCount = periodOrders.Count(o => o.Status == "Completed");
            var pendingOrdersCount = periodOrders.Count(o => o.Status == "Processing" || o.Status == "Pending");
            var cancelledOrdersCount = periodOrders.Count(o => o.Status == "Cancelled");

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
