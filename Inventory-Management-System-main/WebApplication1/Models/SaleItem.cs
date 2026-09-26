using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class SaleItem
    {
        [Key]
        public int SaleItemID { get; set; }

        [Required]
        [ForeignKey(nameof(Sale))]
        public int SaleID { get; set; }

        [Required(ErrorMessage = "Please select a product")]
        [Display(Name = "Product")]
        [ForeignKey(nameof(Product))]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Please specify the quantity")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Unit Price")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        public decimal UnitPrice { get; set; }

        // العلاقات المباشرة (Navigation Properties) مع الفاتورة والمنتج
        public virtual Sale? Sale { get; set; }
        public virtual Product? Product { get; set; }
    }
}