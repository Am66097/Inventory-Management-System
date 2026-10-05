using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Sale
    {
        [Key]
        public int SaleID { get; set; }

        [Required(ErrorMessage = "Sale date is required")]
        [Display(Name = "Sale Date")]
        [DataType(DataType.DateTime)]
        public DateTime SaleDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
        [Range(0, double.MaxValue, ErrorMessage = "Total amount must be greater than or equal to 0")]
        public decimal TotalAmount { get; set; }

        [StringLength(250, ErrorMessage = "Customer info cannot exceed 250 characters")]
        [Display(Name = "Customer Info")]
        public string? CustomerInfo { get; set; }

        // علاقة واحد إلى متعدد (1-to-Many) مع عناصر الفاتورة
        public virtual ICollection<SaleItem> SalesItems { get; set; } = new List<SaleItem>();
    }
}