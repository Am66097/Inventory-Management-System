using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    // تفاصيل (بنود) فاتورة الشراء - Dev 3
    public class PurchaseItem
    {
        [Key]
        public int PurchaseItemID { get; set; }

        // ربط العنصر بالفاتورة الرئيسية
        [Required]
        [ForeignKey(nameof(Purchase))]
        public int PurchaseID { get; set; }
        public virtual Purchase? Purchase { get; set; }

        // ربط العنصر بالمنتج
        [Required(ErrorMessage = "Please select a product.")]
        [Display(Name = "Product")]
        [ForeignKey(nameof(Product))]
        public int ProductID { get; set; }
        public virtual Product? Product { get; set; }

        // الكمية المشتراة
        [Required(ErrorMessage = "Please enter the quantity.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        // سعر الشراء لكل وحدة
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Unit Cost")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit cost must be greater than 0.")]
        public decimal UnitCost { get; set; }
    }
}
