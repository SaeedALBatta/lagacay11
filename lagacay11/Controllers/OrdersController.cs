using lagacay11.Data.Repositories;
using lagacay11.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace lagacay11.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderRepository _orderRepository;

        public OrdersController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // GET: /Orders
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Challenge();
            }

            var orders = await _orderRepository.ListUserOrdersAsync(userId);
            
            var viewModel = new UserOrdersListViewModel
            {
                Orders = orders.Select(o => new OrderSummaryViewModel
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    PaymentReference = o.PaymentReference,
                    Status = o.Status,
                    CustomerName = o.User?.FullName ?? string.Empty
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: /Orders/Details/5
        public async Task<IActionResult> Details(int id)
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
                CustomerName = order.User?.FullName ?? string.Empty,
                CustomerEmail = order.User?.Email ?? string.Empty,
                CustomerPhone = order.User?.PhoneNumber ?? string.Empty,
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

        // GET: /Orders/Invoice/5
        public async Task<IActionResult> Invoice(int id)
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

            var viewModel = new InvoiceViewModel
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                PaymentReference = order.PaymentReference,
                CustomerName = order.User?.FullName ?? string.Empty,
                CustomerEmail = order.User?.Email ?? string.Empty,
                CustomerPhone = order.User?.PhoneNumber ?? string.Empty,
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
    }
}
