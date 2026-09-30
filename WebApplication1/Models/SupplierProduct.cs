using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class SupplierProduct
    {
        [Key]
        public int SupplierProductID { get; set; }

        [Required(ErrorMessage = "Please select a Supplier.")]
        public int SupplierID { get; set; }

        [ForeignKey("SupplierID")]
        public virtual Supplier? Supplier { get; set; }

        [Required(ErrorMessage = "Please select a Product.")]
        public int ProductID { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product? Product { get; set; }

        [Required(ErrorMessage = "Supplier SKU is required.")]
        [StringLength(50)]
        public string SupplierSKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contract Price is required.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ContractPrice { get; set; }

        [Required(ErrorMessage = "Contract Quantity is required.")]
        public int ContractQuantity { get; set; }

        [Required(ErrorMessage = "Lead Time Days is required.")]
        [Range(1, 365, ErrorMessage = "Lead Time must be between 1 and 365 days.")]
        public int LeadTimeDays { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}