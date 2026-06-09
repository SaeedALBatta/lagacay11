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
    public class AdminCategoriesController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;

        public AdminCategoriesController(ICategoryRepository categoryRepository, IProductRepository productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        // GET: /Admin/AdminCategories
        public async Task<IActionResult> Index(string? search)
        {
            var categories = await _categoryRepository.ListAllAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                categories = categories.Where(c => c.Name.ToLower().Contains(s) || c.Description.ToLower().Contains(s)).ToList();
            }

            var categoryViewModels = categories.Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description
            }).ToList();

            var model = new AdminCategoryListViewModel
            {
                Categories = categoryViewModels,
                SelectedSearch = search
            };

            return View(model);
        }

        // GET: /Admin/AdminCategories/Create
        public IActionResult Create()
        {
            return View(new AdminCategoryViewModel());
        }

        // POST: /Admin/AdminCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = new Category
                {
                    Name = model.Name,
                    Slug = string.IsNullOrWhiteSpace(model.Slug)
                        ? model.Name.Trim().ToLower().Replace(" ", "-")
                        : model.Slug.Trim().ToLower().Replace(" ", "-"),
                    Description = model.Description ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                await _categoryRepository.AddAsync(category);

                TempData["SuccessMessage"] = $"Category '{category.Name}' added successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: /Admin/AdminCategories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var model = new AdminCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description
            };

            return View(model);
        }

        // POST: /Admin/AdminCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminCategoryViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                category.Name = model.Name;
                category.Slug = string.IsNullOrWhiteSpace(model.Slug)
                    ? model.Name.Trim().ToLower().Replace(" ", "-")
                    : model.Slug.Trim().ToLower().Replace(" ", "-");
                category.Description = model.Description ?? string.Empty;

                await _categoryRepository.UpdateAsync(category);
                TempData["SuccessMessage"] = $"Category '{category.Name}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: /Admin/AdminCategories/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var model = new AdminCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description
            };

            return View(model);
        }

        // POST: /Admin/AdminCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category != null)
            {
                var productsInCat = await _productRepository.GetAdminProductsAsync(null, id);
                if (productsInCat.Any())
                {
                    TempData["ErrorMessage"] = $"Cannot delete category '{category.Name}' because it has {productsInCat.Count} products mapped to it. Remove those products first.";
                }
                else
                {
                    await _categoryRepository.DeleteAsync(id);
                    TempData["SuccessMessage"] = $"Category '{category.Name}' deleted successfully.";
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
