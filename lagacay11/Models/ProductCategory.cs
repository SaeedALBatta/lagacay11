using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lagacay11.Models
{
    public class ProductCategory
    {
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;
    }
}
