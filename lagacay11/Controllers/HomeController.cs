using lagacay11.Data.Repositories;
using lagacay11.Models;
using lagacay11.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace lagacay11.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITestimonialRepository _testimonialRepository;

        public HomeController(
            ILogger<HomeController> logger,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            ITestimonialRepository testimonialRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _testimonialRepository = testimonialRepository;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Fetch Categories
            var categories = await _categoryRepository.ListAllAsync();
            var categoryViewModels = categories.Take(4).Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description
            }).ToList();

            // 2. Fetch Approved Testimonials
            var testimonials = await _testimonialRepository.ListAllAsync();
            var testimonialViewModels = testimonials.Where(t => t.IsApproved).Take(5).Select(t => new TestimonialViewModel
            {
                Id = t.Id,
                UserFullName = t.User?.FullName ?? "Anonymous",
                UserEmail = t.User?.Email ?? string.Empty,
                Content = t.Content,
                IsApproved = t.IsApproved,
                CreatedAt = t.CreatedAt
            }).ToList();

            // 3. Fetch Retro Arrivals
            var retroArrivals = await _productRepository.GetAvailableProductsAsync(null, "retro-collection", null, null, null, null, "newest");
            var retroViewModels = retroArrivals.Take(4).Select(MapToProductViewModel).ToList();

            // 4. Fetch World Cup 2022 Featured
            var wcFeatured = await _productRepository.GetAvailableProductsAsync(null, "world-cup-2022", null, null, null, null, "newest");
            var wcViewModels = wcFeatured.Take(4).Select(MapToProductViewModel).ToList();

            var viewModel = new HomeIndexViewModel
            {
                Categories = categoryViewModels,
                Testimonials = testimonialViewModels,
                RetroArrivals = retroViewModels,
                WcFeatured = wcViewModels
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> MuseumMoments()
        {
            var maradonaProduct = await _productRepository.GetAvailableProductsAsync("1986 'Hand of God' Match Ball Replica", null, null, null, null, null, null);
            var peleProduct = await _productRepository.GetAvailableProductsAsync("1958 World Cup Final Match Ball Replica", null, null, null, null, null, null);
            var zidaneProduct = await _productRepository.GetAvailableProductsAsync("1998 'Tricolore' Match Ball Replica", null, null, null, null, null, null);

            var messiProduct = await _productRepository.GetAvailableProductsAsync("2022 World Cup Final Match Ball Replica", null, null, null, null, null, null);
            var iniestaProduct = await _productRepository.GetAvailableProductsAsync("2010 World Cup Final Jabulani Replica", null, null, null, null, null, null);
            var ronaldoProduct = await _productRepository.GetAvailableProductsAsync("2002 World Cup Final Fevernova Replica", null, null, null, null, null, null);
            var gotzeProduct = await _productRepository.GetAvailableProductsAsync("2014 World Cup Final Brazuca Replica", null, null, null, null, null, null);

            ViewBag.MaradonaProductId = maradonaProduct.FirstOrDefault()?.Id;
            ViewBag.PeleProductId = peleProduct.FirstOrDefault()?.Id;
            ViewBag.ZidaneProductId = zidaneProduct.FirstOrDefault()?.Id;

            ViewBag.Messi2022ProductId = messiProduct.FirstOrDefault()?.Id;
            ViewBag.Iniesta2010ProductId = iniestaProduct.FirstOrDefault()?.Id;
            ViewBag.Ronaldo2002ProductId = ronaldoProduct.FirstOrDefault()?.Id;
            ViewBag.Gotze2014ProductId = gotzeProduct.FirstOrDefault()?.Id;

            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
