using LibraryManagementSystem.DTOs.Book;
using LibraryManagementSystem.Models;
using static System.Reflection.Metadata.BlobBuilder;

namespace LibraryManagementSystem.Mappers
{
    public class BookMapper
    {
        public static BookDTO FromModel(Book book) 
        {
            var bookCopies = new List<BookCopyResponseDTO>();
            if (book.BookCopies.Any())
            {
                bookCopies = book.BookCopies.Select(BookCopyMapper.FromModel).ToList();
            }
            var bookDto = new BookDTO
            {
                BookId = book.BookId,
                Title = book.Title,
                ISBN = book.Isbn,
                Author = book.Author.AuthorName,
                Category = book.Category.CategoryName,
                PublishedYear = book.PublishedYear,
                AvailableCopies = book.AvailableCopies,
                TotalCopies = book.TotalCopies,
                BookCopies = bookCopies
            };
            return bookDto;
        }
        public static Book ToModel(BookRequestDTO bookDTO)
        {
            var book = new Book
            {
                Title = bookDTO.Title,
                Isbn = bookDTO.ISBN,
                AuthorId = bookDTO.AuthorId,
                CategoryId = bookDTO.CategoryId,
                PublishedYear = bookDTO.PublishedYear,
                AvailableCopies = bookDTO.AvailableCopies,
                TotalCopies = bookDTO.TotalCopies
            };
            return book;
        }
    }
}
