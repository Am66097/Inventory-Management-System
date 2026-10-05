using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        // الوسم NotMapped يمنع Entity Framework من البحث عن عمود Name في SQL
        [NotMapped]
        public string Name
        {
            get => CategoryName;
            set => CategoryName = value;
        }

        public string? Description { get; set; }

        // Navigation Property
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }

















}