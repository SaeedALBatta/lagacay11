using System;
using System.ComponentModel.DataAnnotations;

namespace lagacay11.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; } // 1-5 stars

        [Required]
        public bool IsApproved { get; set; } = false; // Admin must approve reviews

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
