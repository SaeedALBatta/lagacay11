using lagacay11.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace lagacay11.Data.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<Product?> GetByIdWithDetailsAsync(int id);
        Task<List<Product>> GetAvailableProductsAsync(string? search, string? category, string? team, string? club, int? year, string? edition, string? sort);
        Task<List<Product>> GetAdminProductsAsync(string? search, int? categoryId);
        Task<List<Product>> GetRelatedProductsAsync(int currentProductId, List<int> categoryIds, int count);
        Task<int> GetCountAsync();
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        
        // Product Images
        Task AddImageAsync(ProductImage image);
        Task<ProductImage?> GetImageByIdAsync(int imageId);
        Task DeleteImageAsync(ProductImage image);
        Task SetMainImageAsync(int productId, int mainImageId);
    }

    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id);
        Task<Category?> GetBySlugAsync(string slug);
        Task<List<Category>> ListAllAsync();
        Task<int> GetCountAsync();
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }

    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int id);
        Task<Order?> GetByIdWithUserAndItemsAsync(int id, string userId);
        Task<Order?> GetByIdWithUserAndItemsAdminAsync(int id);
        Task<List<Order>> ListUserOrdersAsync(string userId);
        Task<List<Order>> ListAdminOrdersAsync(string? status, DateTime? startDate, DateTime? endDate, string? search);
        Task<int> GetCountAsync();
        Task<decimal> GetTotalRevenueAsync();
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task<bool> HasUserPurchasedProductAsync(string userId, int productId);
    }

    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(int id);
        Task<List<Review>> ListAllAsync();
        Task AddAsync(Review review);
        Task UpdateAsync(Review review);
        Task DeleteAsync(int id);
    }

    public interface ITestimonialRepository
    {
        Task<Testimonial?> GetByIdAsync(int id);
        Task<List<Testimonial>> ListAllAsync();
        Task AddAsync(Testimonial testimonial);
        Task UpdateAsync(Testimonial testimonial);
        Task DeleteAsync(int id);
    }

    public interface IWishlistRepository
    {
        Task<List<Wishlist>> ListUserWishlistAsync(string userId);
        Task<bool> AnyAsync(string userId, int productId);
        Task AddAsync(Wishlist wishlist);
        Task RemoveAsync(string userId, int productId);
    }

    public interface IUserRepository
    {
        Task<List<ApplicationUser>> ListAllAsync(string? search);
        Task<int> GetCountAsync();
        Task<ApplicationUser?> GetByIdAsync(string id);
    }

    public interface ICouponRepository
    {
        Task<Coupon?> GetByIdAsync(int id);
        Task<Coupon?> GetByCodeAsync(string code);
        Task<List<Coupon>> ListAllAsync(string? search);
        Task<int> GetCountAsync();
        Task AddAsync(Coupon coupon);
        Task UpdateAsync(Coupon coupon);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
