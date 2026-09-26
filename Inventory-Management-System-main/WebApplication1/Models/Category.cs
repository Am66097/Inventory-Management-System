using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; } 


        // Navigation Properties 

        // public ICollection<Product> Products { get; set; } 


    }
}
