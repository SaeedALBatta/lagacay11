using System.ComponentModel.DataAnnotations;

namespace lagacay11.Models
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Shipping Address is required.")]
        [StringLength(500, ErrorMessage = "Shipping Address cannot exceed 500 characters.")]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Payment Reference is required.")]
        [StringLength(100, ErrorMessage = "Payment Reference cannot exceed 100 characters.")]
        [Display(Name = "Payment Reference (Transaction ID)")]
        public string PaymentReference { get; set; } = string.Empty;
    }
}
