using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.DTOs.Category
{
    public class CategoryRequestDTO
    {
        [Required(ErrorMessage = "Category Name is required.")]
        [MinLength(3, ErrorMessage = "Category Name should be at least 3 characters long.")]
        [MaxLength(20, ErrorMessage = "Category Name should not exceed 20 characters.")]
        public string CategoryName { get; set; }
    }
}
