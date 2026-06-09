using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace lagacay11.Models.ViewModels
{
    // Category ViewModels
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    // Product ViewModels
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string ClubName { get; set; } = string.Empty;
        public int Year { get; set; }
        public string WorldCupEdition { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string MainImagePath { get; set; } = string.Empty;
        public List<string> ImagePaths { get; set; } = new();
        public List<CategoryViewModel> Categories { get; set; } = new();
        public int OrderCount { get; set; }
    }

    // HomeIndexViewModel
    public class HomeIndexViewModel
    {
        public List<CategoryViewModel> Categories { get; set; } = new();
        public List<ProductViewModel> RetroArrivals { get; set; } = new();
        public List<ProductViewModel> WcFeatured { get; set; } = new();
        public List<TestimonialViewModel> Testimonials { get; set; } = new();
    }

    // TestimonialViewModel
    public class TestimonialViewModel
    {
        public int Id { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ReviewViewModel
    public class ReviewViewModel
    {
        public int Id { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public int Rating { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ProductListViewModel
    public class ProductListViewModel
    {
        public List<ProductViewModel> Products { get; set; } = new();
        public List<CategoryViewModel> Categories { get; set; } = new();
        public List<string> WorldCupEditions { get; set; } = new();
        public List<int> Years { get; set; } = new();
        
        // Search & Filters state
        public string? SelectedSearch { get; set; }
        public string? SelectedCategory { get; set; }
        public string? SelectedTeam { get; set; }
        public string? SelectedClub { get; set; }
        public int? SelectedYear { get; set; }
        public string? SelectedEdition { get; set; }
        public string? SelectedSort { get; set; }
    }

    // ProductDetailsViewModel
    public class ProductDetailsViewModel
    {
        public ProductViewModel Product { get; set; } = new();
        public List<ReviewViewModel> ApprovedReviews { get; set; } = new();
        public double AverageRating { get; set; }
        public List<ProductViewModel> RelatedProducts { get; set; } = new();
        public bool CanUserReview { get; set; }
    }

    // CartItemViewModel
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public decimal Subtotal => Price * Quantity;
    }

    // CartViewModel
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal Total { get; set; }
        
        // Coupon properties
        public string? CouponCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalTotal { get; set; }
    }

    // CheckoutPageViewModel
    public class CheckoutPageViewModel
    {
        public CheckoutViewModel Form { get; set; } = new();
        public List<CartItemViewModel> CartItems { get; set; } = new();
        public decimal CartTotal { get; set; }
        
        // Coupon properties
        public string? CouponCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalTotal { get; set; }
    }

    // OrderSummaryViewModel (Index table)
    public class OrderSummaryViewModel
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentReference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
    }

    // UserOrdersListViewModel
    public class UserOrdersListViewModel
    {
        public List<OrderSummaryViewModel> Orders { get; set; } = new();
    }

    // OrderItemViewModel
    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int ProductYear { get; set; }
        public string WorldCupEdition { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }

    // OrderDetailViewModel
    public class OrderDetailViewModel
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public List<OrderItemViewModel> OrderItems { get; set; } = new();
    }

    // InvoiceViewModel
    public class InvoiceViewModel
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public List<OrderItemViewModel> OrderItems { get; set; } = new();
    }

    // WishlistViewModel
    public class WishlistViewModel
    {
        public List<ProductViewModel> Products { get; set; } = new();
    }

    // AdminDashboardViewModel
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<OrderSummaryViewModel> RecentOrders { get; set; } = new();
        public List<decimal> SalesTrendData { get; set; } = new();
        public List<string> SalesTrendLabels { get; set; } = new();
        public int CompletedOrdersCount { get; set; }
        public int PendingOrdersCount { get; set; }
        public int CancelledOrdersCount { get; set; }
    }

    // AdminProductListViewModel
    public class AdminProductListViewModel
    {
        public List<ProductViewModel> Products { get; set; } = new();
        public List<CategoryViewModel> Categories { get; set; } = new();
        public string? SelectedSearch { get; set; }
        public int? SelectedCategoryId { get; set; }
    }

    // AdminProductCreateViewModel
    public class AdminProductCreateViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 999999.99)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required.")]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [StringLength(100)]
        public string TeamName { get; set; } = string.Empty;

        [StringLength(100)]
        public string ClubName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year is required.")]
        [Range(1800, 2100)]
        public int Year { get; set; }

        [StringLength(100)]
        public string WorldCupEdition { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;

        [Required(ErrorMessage = "Select at least one category.")]
        public int[] SelectedCategoryIds { get; set; } = Array.Empty<int>();

        public List<IFormFile> Images { get; set; } = new();
    }

    // AdminProductEditViewModel
    public class AdminProductEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 999999.99)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required.")]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [StringLength(100)]
        public string TeamName { get; set; } = string.Empty;

        [StringLength(100)]
        public string ClubName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year is required.")]
        [Range(1800, 2100)]
        public int Year { get; set; }

        [StringLength(100)]
        public string WorldCupEdition { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;

        [Required(ErrorMessage = "Select at least one category.")]
        public int[] SelectedCategoryIds { get; set; } = Array.Empty<int>();

        public List<IFormFile> Images { get; set; } = new();

        public List<ProductImageViewModel> ExistingImages { get; set; } = new();
    }

    public class ProductImageViewModel
    {
        public int Id { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public bool IsMain { get; set; }
    }

    // AdminCategoryListViewModel
    public class AdminCategoryListViewModel
    {
        public List<CategoryViewModel> Categories { get; set; } = new();
        public string? SelectedSearch { get; set; }
    }

    // AdminCategoryViewModel (Create/Edit)
    public class AdminCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug is required.")]
        [StringLength(100)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
    }

    // AdminOrderListViewModel
    public class AdminOrderListViewModel
    {
        public List<OrderSummaryViewModel> Orders { get; set; } = new();
        public string? SelectedStatus { get; set; }
        public string? SelectedStartDate { get; set; }
        public string? SelectedEndDate { get; set; }
        public string? SelectedSearch { get; set; }
    }

    // AdminReviewListViewModel
    public class AdminReviewListViewModel
    {
        public List<AdminReviewViewModel> Reviews { get; set; } = new();
    }

    public class AdminReviewViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public int Rating { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // AdminTestimonialListViewModel
    public class AdminTestimonialListViewModel
    {
        public List<TestimonialViewModel> Testimonials { get; set; } = new();
    }

    // AdminUserListViewModel
    public class AdminUserListViewModel
    {
        public List<UserViewModel> Users { get; set; } = new();
        public string? SelectedSearch { get; set; }
    }

    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class AdminUserDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ShippingStreet { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingPostalCode { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string Role { get; set; } = "Customer";
        
        // stats
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AvgOrderValue { get; set; }
        public DateTime? LastOrderDate { get; set; }
        
        // orders
        public List<OrderSummaryViewModel> RecentOrders { get; set; } = new();
    }

    // AdminCouponListViewModel
    public class AdminCouponListViewModel
    {
        public List<Coupon> Coupons { get; set; } = new();
        public string? SelectedSearch { get; set; }
        public string? SelectedStatus { get; set; }
        public int ActiveCouponsCount { get; set; }
        public decimal TotalDiscountedValue { get; set; }
        public int TotalUsesCount { get; set; }
    }

    // AdminCouponViewModel (Create/Edit)
    public class AdminCouponViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Coupon code is required.")]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Code must be alphanumeric with no spaces.")]
        public string Code { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = "percentage"; // percentage / fixed

        [Required(ErrorMessage = "Discount value is required.")]
        [Range(0.01, 999999.99, ErrorMessage = "Value must be positive.")]
        public decimal Value { get; set; }

        public DateTime? ExpirationDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Usage limit must be at least 1.")]
        public int? UsageLimit { get; set; }

        public int UsageCount { get; set; }

        [Range(0.00, 999999.99, ErrorMessage = "Minimum purchase must be a positive amount.")]
        public decimal? MinimumPurchase { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
