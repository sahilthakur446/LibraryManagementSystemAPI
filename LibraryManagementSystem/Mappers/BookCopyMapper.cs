using LibraryManagementSystem.DTOs.Book;
using LibraryManagementSystem.Models;
using static System.Reflection.Metadata.BlobBuilder;

namespace LibraryManagementSystem.Mappers
{
    public class BookCopyMapper
    {
        public static BookCopyResponseDTO FromModel(BookCopy bookCopy)
        
        {
            var bookCopyDto = new BookCopyResponseDTO
            {
                CopyId = bookCopy.CopyId,
                BookId = bookCopy.BookId,
                IsAvailable = bookCopy.IsAvailable,
                Book = bookCopy.Book != null ? BookMapper.FromModel(bookCopy.Book) : null
            };
            return bookCopyDto;
        }
        public static BookCopy ToModel(BookCopyRequestDTO bookCopyDto)
        {
            var bookCopy = new BookCopy
            {
                BookId = bookCopyDto.BookId,
                IsAvailable = bookCopyDto.IsAvailable,
            };
            return bookCopy;
        }
    }
}
