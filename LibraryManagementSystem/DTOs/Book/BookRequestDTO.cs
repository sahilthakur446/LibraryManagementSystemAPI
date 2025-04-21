using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.DTOs.Book
{
    public class BookRequestDTO
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required]
        public int AuthorId { get; set; }

        [Required]
        [MaxLength(20)]
        public string ISBN { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        [Required]
        [Range(1500, 2025)]
        public int PublishedYear { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int TotalCopies { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int AvailableCopies { get; set; }
    }
}
