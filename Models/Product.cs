using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Product Name is required.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // للربط مع الأجزاء القديمة التي تستخدم ProductName
        public string ProductName
        {
            get => Name;
            set => Name = value;
        }

        public string Description { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // للربط مع الأجزاء القديمة التي تستخدم UnitPrice
        public decimal UnitPrice
        {
            get => Price;
            set => Price = value;
        }

        [Required(ErrorMessage = "Stock Quantity is required.")]
        public int StockQuantity { get; set; }

        public int LowStockThreshold { get; set; } = 5;

        // العلاقة مع الـ Category
        [Required(ErrorMessage = "Please select a Category.")]
        public int CategoryID { get; set; }

        [ForeignKey("CategoryID")]
        public virtual Category? Category { get; set; }

        // العلاقة مع SupplierProduct
        public virtual ICollection<SupplierProduct> SupplierProducts { get; set; } = new List<SupplierProduct>();
    }
}