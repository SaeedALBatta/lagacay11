using System.ComponentModel.DataAnnotations;

namespace lagacay11.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        [Required]
        [StringLength(500)]
        public string ImagePath { get; set; } = string.Empty;

        public bool IsMain { get; set; } = false;

        public int SortOrder { get; set; } = 0;
    }
}
