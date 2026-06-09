using lagacay11.Data.Repositories;
using lagacay11.Models;
using lagacay11.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace lagacay11.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminProductsController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Admin/AdminProducts
        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            var products = await _productRepository.GetAdminProductsAsync(search, categoryId);
            var categories = await _categoryRepository.ListAllAsync();

            var viewModel = new AdminProductListViewModel
            {
                Products = products.Select(MapToProductViewModel).ToList(),
                Categories = categories.Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug
                }).ToList(),
                SelectedSearch = search,
                SelectedCategoryId = categoryId
            };

            return View(viewModel);
        }

        // GET: /Admin/AdminProducts/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryRepository.ListAllAsync();
            return View(new AdminProductCreateViewModel());
        }

        // POST: /Admin/AdminProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminProductCreateViewModel model)
        {
            if (model.SelectedCategoryIds == null || model.SelectedCategoryIds.Length == 0)
            {
                ModelState.AddModelError("", "Every jersey must belong to at least one category.");
            }

            // Image format validation (.jpg .jpeg .png .webp only)
            var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (model.Images != null)
            {
                foreach (var img in model.Images)
                {
                    var ext = Path.GetExtension(img.FileName).ToLower();
                    if (!permittedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError("", $"File type '{ext}' is not permitted. Only JPG, JPEG, PNG, and WEBP images are allowed.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    StockQuantity = model.StockQuantity,
                    TeamName = model.TeamName ?? string.Empty,
                    ClubName = model.ClubName ?? string.Empty,
                    Year = model.Year,
                    WorldCupEdition = model.WorldCupEdition ?? string.Empty,
                    IsAvailable = model.IsAvailable,
                    CreatedAt = DateTime.UtcNow
                };

                await _productRepository.AddAsync(product); // Add to get ID

                // Save Category relations
                if (model.SelectedCategoryIds != null)
                {
                    foreach (var catId in model.SelectedCategoryIds)
                    {
                        product.ProductCategories.Add(new ProductCategory
                        {
                            ProductId = product.Id,
                            CategoryId = catId
                        });
                    }
                }
                await _productRepository.UpdateAsync(product);

                // Handle Image uploads to wwwroot/uploads/products/
                if (model.Images != null && model.Images.Count > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "products");
                    Directory.CreateDirectory(uploadsFolder);

                    bool isFirst = true;
                    int sort = 1;
                    foreach (var image in model.Images)
                    {
                        if (image.Length > 0)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await image.CopyToAsync(fileStream);
                            }

                            await _productRepository.AddImageAsync(new ProductImage
                            {
                                ProductId = product.Id,
                                ImagePath = "/uploads/products/" + uniqueFileName,
                                IsMain = isFirst,
                                SortOrder = sort++
                            });

                            isFirst = false;
                        }
                    }
                }
                else
                {
                    // Add default placeholder image
                    await _productRepository.AddImageAsync(new ProductImage
                    {
                        ProductId = product.Id,
                        ImagePath = "/uploads/products/placeholder.jpg",
                        IsMain = true,
                        SortOrder = 1
                    });
                }

                TempData["SuccessMessage"] = $"Successfully added {product.Name}!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _categoryRepository.ListAllAsync();
            return View(model);
        }

        // GET: /Admin/AdminProducts/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdWithDetailsAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var viewModel = new AdminProductEditViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                TeamName = product.TeamName,
                ClubName = product.ClubName,
                Year = product.Year,
                WorldCupEdition = product.WorldCupEdition,
                IsAvailable = product.IsAvailable,
                SelectedCategoryIds = product.ProductCategories.Select(pc => pc.CategoryId).ToArray(),
                ExistingImages = product.ProductImages.OrderBy(pi => pi.SortOrder).Select(pi => new ProductImageViewModel
                {
                    Id = pi.Id,
                    ImagePath = pi.ImagePath,
                    IsMain = pi.IsMain
                }).ToList()
            };

            ViewBag.Categories = await _categoryRepository.ListAllAsync();
            return View(viewModel);
        }

        // POST: /Admin/AdminProducts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminProductEditViewModel model, int? mainImageId)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (model.SelectedCategoryIds == null || model.SelectedCategoryIds.Length == 0)
            {
                ModelState.AddModelError("", "Every jersey must belong to at least one category.");
            }

            // Image format validation (.jpg .jpeg .png .webp only)
            var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (model.Images != null)
            {
                foreach (var img in model.Images)
                {
                    var ext = Path.GetExtension(img.FileName).ToLower();
                    if (!permittedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError("", $"File type '{ext}' is not permitted. Only JPG, JPEG, PNG, and WEBP images are allowed.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var product = await _productRepository.GetByIdWithDetailsAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                // Update properties
                product.Name = model.Name;
                product.Description = model.Description;
                product.Price = model.Price;
                product.StockQuantity = model.StockQuantity;
                product.TeamName = model.TeamName ?? string.Empty;
                product.ClubName = model.ClubName ?? string.Empty;
                product.Year = model.Year;
                product.WorldCupEdition = model.WorldCupEdition ?? string.Empty;
                product.IsAvailable = model.IsAvailable;

                // Sync categories
                product.ProductCategories.Clear();
                if (model.SelectedCategoryIds != null)
                {
                    foreach (var catId in model.SelectedCategoryIds)
                    {
                        product.ProductCategories.Add(new ProductCategory
                        {
                            ProductId = id,
                            CategoryId = catId
                        });
                    }
                }
                await _productRepository.UpdateAsync(product);

                // Handle uploads
                if (model.Images != null && model.Images.Count > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "products");
                    Directory.CreateDirectory(uploadsFolder);

                    int sort = product.ProductImages.Any() ? product.ProductImages.Max(pi => pi.SortOrder) + 1 : 1;
                    foreach (var image in model.Images)
                    {
                        if (image.Length > 0)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await image.CopyToAsync(fileStream);
                            }

                            await _productRepository.AddImageAsync(new ProductImage
                            {
                                ProductId = id,
                                ImagePath = "/uploads/products/" + uniqueFileName,
                                IsMain = !product.ProductImages.Any(pi => pi.IsMain),
                                SortOrder = sort++
                            });
                        }
                    }
                }

                // Sync main image
                if (mainImageId.HasValue)
                {
                    await _productRepository.SetMainImageAsync(id, mainImageId.Value);
                }

                TempData["SuccessMessage"] = $"Successfully updated {product.Name}!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _categoryRepository.ListAllAsync();
            return View(model);
        }

        // POST: /Admin/AdminProducts/DeleteImage/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId, int productId)
        {
            var image = await _productRepository.GetImageByIdAsync(imageId);
            if (image != null)
            {
                if (image.ImagePath != "/uploads/products/placeholder.jpg" && !image.ImagePath.Contains("placeholder"))
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                await _productRepository.DeleteImageAsync(image);

                if (image.IsMain)
                {
                    var product = await _productRepository.GetByIdWithDetailsAsync(productId);
                    var nextImg = product?.ProductImages.FirstOrDefault();
                    if (nextImg != null)
                    {
                        nextImg.IsMain = true;
                        await _productRepository.UpdateAsync(product!);
                    }
                }
                TempData["SuccessMessage"] = "Image deleted successfully.";
            }

            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        // GET: /Admin/AdminProducts/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdWithDetailsAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(MapToProductViewModel(product));
        }

        // POST: /Admin/AdminProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productRepository.GetByIdWithDetailsAsync(id);
            if (product != null)
            {
                if (product.OrderItems.Any())
                {
                    product.IsAvailable = false;
                    await _productRepository.UpdateAsync(product);
                    TempData["SuccessMessage"] = $"Soft deleted {product.Name} (marked unavailable) as it is present in client orders.";
                }
                else
                {
                    // Hard Delete files
                    foreach (var img in product.ProductImages)
                    {
                        if (img.ImagePath != "/uploads/products/placeholder.jpg" && !img.ImagePath.Contains("placeholder"))
                        {
                            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, img.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                        }
                    }

                    await _productRepository.DeleteAsync(id);
                    TempData["SuccessMessage"] = $"Successfully deleted {product.Name}!";
                }
            }

            return RedirectToAction(nameof(Index));
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
                }).ToList(),
                OrderCount = p.OrderItems?.Count ?? 0
            };
        }
    }
}
