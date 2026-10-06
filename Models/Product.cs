using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Product Name is required.")]
        [StringLength(100, ErrorMessage = "Product Name cannot exceed 100 characters.")]
        public string ProductName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "Unit Price is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Unit Price cannot be negative.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Stock Quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock Quantity cannot be negative.")]
        public int StockQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Low Stock Threshold cannot be negative.")]
        public int LowStockThreshold { get; set; } = 5;

        // Relationship with Category
        [Required(ErrorMessage = "Please select a Category.")]
        public int CategoryID { get; set; }

        [ForeignKey("CategoryID")]
        public virtual Category? Category { get; set; }

        // Relationship with SupplierProduct
        public virtual ICollection<SupplierProduct> SupplierProducts { get; set; } = new List<SupplierProduct>();
    }
}