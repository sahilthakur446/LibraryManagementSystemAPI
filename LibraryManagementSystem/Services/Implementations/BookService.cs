using LibraryManagementSystem.DTOs.Book;
using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.Mappers;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services.Interfaces;

public class BookService : IBookService
{
    private readonly IBookRepository bookRepository;
    private readonly ILogger<BookService> logger;

    public BookService(IBookRepository bookRepository, ILogger<BookService> logger)
    {
        this.bookRepository = bookRepository;
        this.logger = logger;
    }

    public async Task<bool> AddBook(BookRequestDTO BookDTO)
    {
        if (BookDTO.TotalCopies < BookDTO.AvailableCopies)
        {
            logger.LogWarning("Total copies cannot be less than available copies");
            throw new BusinessExceptions("Total copies cannot be less than available copies", StatusCodes.Status400BadRequest);
        }

        try
        {
            if (BookDTO == null)
            {
                logger.LogWarning("Please provide valid Book details");
                throw new BusinessExceptions("Invalid book details provided", StatusCodes.Status400BadRequest);
            }

            var book = BookMapper.ToModel(BookDTO);

            return await bookRepository.Insert(book);
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "Error occurred while adding book");
            throw new BusinessExceptions("Error occurred while adding book", StatusCodes.Status500InternalServerError);
        }

    }

    public async Task<bool> DeleteBook(int id)
    {
        try
        {
            return await bookRepository.Delete(id);
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "Error deleting book");
            throw new BusinessExceptions("Error deleting book", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<IList<BookDTO>> GetAllBooks()
    {
        try
        {
            var books = await bookRepository.GetAll();
            if (books == null || !books.Any())
            {
                logger.LogInformation("No books found");
                throw new BusinessExceptions("No books found", StatusCodes.Status404NotFound);
            }
            var allBooks = books.Select(b => BookMapper.FromModel(b!));
            return allBooks.ToList();
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "Service: Error fetching books");
            throw new BusinessExceptions("Service: Error fetching books Ex", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<BookDTO> GetBookById(int id)
    {
        try
        {
            Book? book = await bookRepository.GetById(id);
            if (book == null)
            {
                logger.LogWarning("Book with ID {Id} not found", id);
                throw new BusinessExceptions("Book not found", StatusCodes.Status404NotFound);
            }

            return BookMapper.FromModel(book);
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "Error fetching book by ID");
            throw new BusinessExceptions("Error fetching book", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<bool> UpdateBook(int id, BookRequestDTO BookDTO)
    {
        if (BookDTO.TotalCopies < BookDTO.AvailableCopies)
        {
            logger.LogWarning("Total copies cannot be less than available copies");
            throw new BusinessExceptions("Total copies cannot be less than available copies", StatusCodes.Status400BadRequest);
        }

        try
        {
            var existingBook = await bookRepository.GetById(id);
            if (existingBook == null)
            {
                logger.LogWarning("Book with ID {Id} not found", id);
                throw new BusinessExceptions("Book not found", StatusCodes.Status404NotFound);
            }

            existingBook.Title = BookDTO.Title;
            existingBook.AuthorId = BookDTO.AuthorId;
            existingBook.Isbn = BookDTO.ISBN;
            existingBook.CategoryId = BookDTO.CategoryId;
            existingBook.PublishedYear = BookDTO.PublishedYear;
            existingBook.TotalCopies = BookDTO.TotalCopies;
            existingBook.AvailableCopies = BookDTO.AvailableCopies;

            return await bookRepository.Update(existingBook);
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "Error updating book");
            throw new BusinessExceptions("Error updating book", StatusCodes.Status500InternalServerError);
        }
    }
}
