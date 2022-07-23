using System.ComponentModel.DataAnnotations;

namespace BookHeapWeb.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "is required")]
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
