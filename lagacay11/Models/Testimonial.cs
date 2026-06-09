using System;
using System.ComponentModel.DataAnnotations;

namespace lagacay11.Models
{
    public class Testimonial
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        public bool IsApproved { get; set; } = false; // Admin must approve testimonials

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
