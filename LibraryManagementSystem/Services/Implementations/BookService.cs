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
        try
        {
            if (BookDTO == null)
            {
                logger.LogWarning("Please provide valid Book details");
                throw new BusinessExceptions("Invalid book details provided", StatusCodes.Status400BadRequest);
            }
            BookDTO.AvailableCopies = BookDTO.TotalCopies;
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

    public async Task<IList<BookDTO>> AddBooks(List<BookRequestDTO> bookDTOs)
    {
        try
        {
            if (bookDTOs == null || bookDTOs.Count == 0)
            {
                logger.LogWarning("No book details provided.");
                throw new BusinessExceptions("No book details provided", StatusCodes.Status400BadRequest);
            }

            var books = bookDTOs.Select(dto =>
            {
                dto.AvailableCopies = dto.TotalCopies;
                return BookMapper.ToModel(dto);
            }).ToList();

            var addedBooks = await bookRepository.AddBooks(books);
            return addedBooks.Select(BookMapper.FromModel).ToList();
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "Error occurred while adding multiple books");
            throw new BusinessExceptions("Error occurred while adding multiple books", StatusCodes.Status500InternalServerError);
        }
    }
    public async Task<IList<BookDTO>> GetAllBooksWithCopies()
    {
        try
        {
            var books = await bookRepository.GetAllBooksWithCopies();
            if (books == null || books.Count == 0)
            {
                logger.LogInformation("No books with copies found.");
                throw new BusinessExceptions("No books with copies found", StatusCodes.Status404NotFound);
            }

            return books.Select(BookMapper.FromModel).ToList();
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "Error fetching books with copies");
            throw new BusinessExceptions("Error fetching books with copies", StatusCodes.Status500InternalServerError);
        }
    }
    public async Task<BookDTO> GetBookWithCopies(int id)
    {
        try
        {
            var book = await bookRepository.GetBookWithCopies(id);
            if (book == null)
            {
                logger.LogWarning("Book with ID {Id} not found.", id);
                throw new BusinessExceptions("Book not found", StatusCodes.Status404NotFound);
            }

            return BookMapper.FromModel(book);
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "Error fetching book with copies");
            throw new BusinessExceptions("Error fetching book with copies", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<BookCopyResponseDTO> GetBookCopyById(int bookCopyId)
    {
        try
        {
            var bookCopy = await bookRepository.GetBookCopyById(bookCopyId);

            if (bookCopy == null)
            {
                logger.LogWarning("Book copy not found. BookCopyId: {BookCopyId}", bookCopyId);
                throw new BusinessExceptions($"Book copy not found with ID {bookCopyId}", StatusCodes.Status404NotFound);
            }
            bookCopy.Book.BookCopies = null;
            return BookCopyMapper.FromModel(bookCopy);
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "Repository exception occurred while retrieving book copy with ID {BookCopyId}", bookCopyId);
            throw new BusinessExceptions("An error occurred while retrieving the book copy.", StatusCodes.Status500InternalServerError);
        }
    }


    public async Task<bool> AddBookCopy(BookCopyRequestDTO bookCopyDTO)
    {
        try
        {
            var book = await bookRepository.GetById(bookCopyDTO.BookId);

            if (book == null)
            {
                // Log warning and throw business exception if book not found
                logger.LogWarning("Book not found in Add Book Copy method. BookId: {BookId}", bookCopyDTO.BookId);
                throw new BusinessExceptions($"Book not found with ID {bookCopyDTO.BookId}", StatusCodes.Status404NotFound);
            }

            // Update TotalCopies and AvailableCopies if book is found
            book.TotalCopies += 1;

            if (bookCopyDTO.IsAvailable)
            {
                book.AvailableCopies += 1;
            }

            // Map the DTO to the BookCopy model
            var bookCopy = BookCopyMapper.ToModel(bookCopyDTO);

            // Add the new book copy and return the result
            return await bookRepository.AddBookCopy(bookCopy, book);
        }
        catch (RepositoryException ex)
        {
            // Log repository-specific errors
            logger.LogError(ex, "Error adding new book copy");
            throw new BusinessExceptions("Error adding new book copy", StatusCodes.Status500InternalServerError);
        }
    }
    public async Task<bool> UpdateBookCopy(int bookCopyId, BookCopyRequestDTO bookCopyDto)
    {
        try
        {
            var bookCopy = await bookRepository.GetBookCopyById(bookCopyId);
            if (bookCopy == null)
                throw new BusinessExceptions("Book copy not found", StatusCodes.Status404NotFound);

            // If BookId has changed, adjust the Total and Available copies
            if (bookCopy.BookId != bookCopyDto.BookId)
            {
                var oldBook = await bookRepository.GetById(bookCopy.BookId);
                if (oldBook == null)
                    throw new BusinessExceptions("Old book not found", StatusCodes.Status404NotFound);

                oldBook.TotalCopies -= 1;
                if (bookCopy.IsAvailable)
                {
                    oldBook.AvailableCopies = oldBook.AvailableCopies - 1;
                }

                await bookRepository.Update(oldBook);

                var newBook = await bookRepository.GetById(bookCopyDto.BookId);
                if (newBook == null)
                    throw new BusinessExceptions("New book not found", StatusCodes.Status404NotFound);

                newBook.TotalCopies += 1;
                if (bookCopyDto.IsAvailable)
                {
                    newBook.AvailableCopies = newBook.AvailableCopies + 1;
                }

                bookCopy.Book = newBook;
            }
            else
            {
                // BookId hasn't changed but availability has
                if (!bookCopyDto.IsAvailable && bookCopy.IsAvailable)
                    bookCopy.Book.AvailableCopies -= 1;
                else if (bookCopyDto.IsAvailable && !bookCopy.IsAvailable)
                    bookCopy.Book.AvailableCopies += 1;
            }

            // Update the bookCopy values
            bookCopy.BookId = bookCopyDto.BookId;
            bookCopy.IsAvailable = bookCopyDto.IsAvailable;

            return await bookRepository.UpdateBookCopy(bookCopy, bookCopy.Book);
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "Error updating book copy");
            throw new BusinessExceptions("Error updating book copy", StatusCodes.Status500InternalServerError);
        }
    }
    public async Task<bool> DeleteBookCopy(int bookCopyId)
    {
        try
        {
            var bookCopy = await bookRepository.GetBookCopyById(bookCopyId);
            if (bookCopy == null)
            {
                logger.LogWarning("Attempted to delete non-existing book copy with ID: {BookCopyId}", bookCopyId);
                throw new BusinessExceptions($"Book copy with ID {bookCopyId} not found.", StatusCodes.Status404NotFound);
            }

            // Update the related Book's copy counts
            bookCopy.Book.TotalCopies -= 1;

            if (bookCopy.IsAvailable)
            {
                bookCopy.Book.AvailableCopies -= 1;
            }

            return await bookRepository.DeleteBookCopy(bookCopy, bookCopy.Book);
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "Error deleting book copy with ID: {BookCopyId}", bookCopyId);
            throw new BusinessExceptions("Error deleting book copy", StatusCodes.Status500InternalServerError);
        }
    }

}
