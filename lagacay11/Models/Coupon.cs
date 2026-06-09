using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lagacay11.Models
{
    public class Coupon
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Type { get; set; } = "percentage"; // percentage / fixed

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Value { get; set; }

        public string Description { get; set; } = string.Empty;

        public DateTime? ExpirationDate { get; set; }

        public int? UsageLimit { get; set; }

        public int UsageCount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinimumPurchase { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
