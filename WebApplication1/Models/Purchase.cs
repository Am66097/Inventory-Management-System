using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    // رأس فاتورة الشراء - Dev 3
    public class Purchase
    {
        [Key]
        public int PurchaseID { get; set; }

        // المورد المرتبط بالفاتورة
        [Required(ErrorMessage = "Please select a Supplier.")]
        [Display(Name = "Supplier")]
        public int SupplierID { get; set; }

        [ForeignKey("SupplierID")]
        public virtual Supplier? Supplier { get; set; }

        [Required]
        [Display(Name = "Purchase Date")]
        [DataType(DataType.DateTime)]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        // يُحسب أوتوماتيك من مجموع عناصر الفاتورة
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        // علاقة 1-to-Many مع عناصر الفاتورة
        public virtual ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    }
}
