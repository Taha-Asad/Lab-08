using System.ComponentModel.DataAnnotations;

namespace PMS_CRUD_Razor.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product title is required.")]
        [StringLength(50)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(50)]
        public string description { get; set; } = string.Empty;

       
        [StringLength(255)]
        public string image { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Price must be zero or greater.")]
        public int price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be zero or greater.")]
        public int quantity { get; set; }
 
        public string status { get; set; } = string.Empty;
    }
}