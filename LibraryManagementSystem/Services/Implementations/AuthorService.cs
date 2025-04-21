using LibraryManagementSystem.DTOs.Author;
using LibraryManagementSystem.DTOs.Book;
using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository authorRepository;
    private readonly ILogger<AuthorService> logger;

    public AuthorService(IAuthorRepository authorRepository, ILogger<AuthorService> logger)
    {
        this.authorRepository = authorRepository;
        this.logger = logger;
    }

    public async Task<bool> AddAuthor(AuthorRequestDTO authorDTO)
    {
        logger.LogInformation("AddAuthor invoked with name: {AuthorName}", authorDTO?.AuthorName);

        if (authorDTO == null || string.IsNullOrWhiteSpace(authorDTO.AuthorName))
        {
            logger.LogWarning("AddAuthor failed: Invalid input.");
            throw new BusinessExceptions("Please provide a valid author name", StatusCodes.Status400BadRequest);
        }

        try
        {
            var newAuthor = new Author { AuthorName = authorDTO.AuthorName };
            bool isInserted = await authorRepository.Insert(newAuthor);

            logger.LogInformation("AddAuthor success: Author '{AuthorName}' inserted: {Result}", authorDTO.AuthorName, isInserted);
            return isInserted;
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "RepositoryException in AddAuthor for name '{AuthorName}'", authorDTO.AuthorName);
            throw new BusinessExceptions("Error adding author", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<bool> DeleteAuthor(int id)
    {
        logger.LogInformation("DeleteAuthor invoked for ID: {Id}", id);

        if (id <= 0)
        {
            logger.LogWarning("DeleteAuthor failed: Invalid ID {Id}", id);
            throw new BusinessExceptions("Please provide a valid ID", StatusCodes.Status400BadRequest);
        }

        try
        {
            bool result = await authorRepository.Delete(id);
            if (!result)
            {
                logger.LogInformation("DeleteAuthor failed: Author not found or already deleted. ID: {Id}", id);
                throw new BusinessExceptions("Author not found or cannot be deleted", StatusCodes.Status404NotFound);
            }

            logger.LogInformation("DeleteAuthor success: ID {Id}", id);
            return result;
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "RepositoryException in DeleteAuthor for ID {Id}", id);
            throw new BusinessExceptions("Error deleting author", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<IEnumerable<AuthorDTO>> GetAllAuthors()
    {
        logger.LogInformation("GetAllAuthors invoked");

        try
        {
            var authors = await authorRepository.GetAll();
            var result = authors.Select(a => new AuthorDTO { AuthorId = a.AuthorId, AuthorName = a.AuthorName });

            logger.LogInformation("GetAllAuthors success: {Count} authors retrieved", result.Count());
            return result;
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "RepositoryException in GetAllAuthors");
            throw new BusinessExceptions("Error fetching authors", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<AuthorDTO?> GetAuthorById(int id)
    {
        logger.LogInformation("GetAuthorById invoked with ID: {Id}", id);

        if (id <= 0)
        {
            logger.LogWarning("GetAuthorById failed: Invalid ID {Id}", id);
            throw new BusinessExceptions("Invalid author ID", StatusCodes.Status400BadRequest);
        }

        try
        {
            var author = await authorRepository.GetById(id);
            if (author == null)
            {
                logger.LogInformation("GetAuthorById: No author found for ID {Id}", id);
                throw new BusinessExceptions("Author not found", StatusCodes.Status404NotFound);
            }

            logger.LogInformation("GetAuthorById success: Author found for ID {Id}", id);
            return new AuthorDTO { AuthorId = author.AuthorId, AuthorName = author.AuthorName };
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "RepositoryException in GetAuthorById for ID {Id}", id);
            throw new BusinessExceptions("Error fetching author", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<List<BookDTO>> GetBooksByAuthor(int id)
    {
        logger.LogInformation("GetBooksByAuthor invoked for ID: {Id}", id);

        if (id <= 0)
        {
            logger.LogWarning("GetBooksByAuthor failed: Invalid ID {Id}", id);
            throw new BusinessExceptions("Invalid author ID", StatusCodes.Status400BadRequest);
        }

        try
        {
            var books = await authorRepository.GetBooksByAuthorId(id);
            if (books == null || !books.Any())
            {
                logger.LogInformation("GetBooksByAuthor: No books found for author ID {Id}", id);
                throw new BusinessExceptions("No books found for this author", StatusCodes.Status404NotFound);
            }

            var result = books.Select(b => new BookDTO { BookId = b.BookId, Title = b.Title }).ToList();
            logger.LogInformation("GetBooksByAuthor success: {Count} books found for author ID {Id}", result.Count, id);
            return result;
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "RepositoryException in GetBooksByAuthor for ID {Id}", id);
            throw new BusinessExceptions("Error fetching books", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<bool> UpdateAuthor(int id, AuthorRequestDTO authorDTO)
    {
        logger.LogInformation("UpdateAuthor invoked for ID: {Id}, New Name: {Name}", id, authorDTO?.AuthorName);

        if (id <= 0 || authorDTO == null || string.IsNullOrWhiteSpace(authorDTO.AuthorName))
        {
            logger.LogWarning("UpdateAuthor failed: Invalid input for ID {Id}", id);
            throw new BusinessExceptions("Invalid input data", StatusCodes.Status400BadRequest);
        }

        try
        {
            var author = await authorRepository.GetById(id);
            if (author == null)
            {
                logger.LogInformation("UpdateAuthor failed: Author not found with ID {Id}", id);
                throw new BusinessExceptions("Author not found", StatusCodes.Status404NotFound);
            }

            author.AuthorName = authorDTO.AuthorName;
            bool updated = await authorRepository.Update(author);

            logger.LogInformation("UpdateAuthor success: Author ID {Id} updated", id);
            return updated;
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "RepositoryException in UpdateAuthor for ID {Id}", id);
            throw new BusinessExceptions("Error updating author", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<bool> DeleteBooksByAuthor(int id)
    {
        if (id <= 0)
        {
            logger.LogWarning("DeleteBooksByAuthor failed: Invalid author ID {Id}", id);
            throw new BusinessExceptions("Please provide a valid author ID", StatusCodes.Status400BadRequest);
        }

        try
        {
            bool result = await authorRepository.DeleteBooksByAuthorId(id);
            logger.LogInformation("DeleteBooksByAuthor result for ID {Id}: {Result}", id, result);
            return result;
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "RepositoryException in DeleteBooksByAuthor for ID {Id}", id);
            throw new BusinessExceptions("Error deleting books", StatusCodes.Status500InternalServerError);
        }
    }
}
