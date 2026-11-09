using Library.Application.DTOs;
using Library.Application.Exceptions;
using Library.Application.Interfaces;
using Library.Application.Mappers;
using Library.Domain.Entities;

namespace Library.Application.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _books;
    private readonly IAuthorRepository _authors;
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _uow;

    public BookService(IBookRepository books, IAuthorRepository authors, ICategoryRepository categories, IUnitOfWork uow)
    {
        _books = books;
        _authors = authors;
        _categories = categories;
        _uow = uow;
    }

    public async Task<List<BookDto>> SearchAsync(string? title, int? categoryId) =>
        (await _books.SearchAsync(title, categoryId)).Select(b => b.ToDto()).ToList();

    public async Task<BookDto> GetByIdAsync(int id) =>
        (await _books.GetByIdAsync(id) ?? throw new NotFoundException("Book not found.")).ToDto();

    public async Task<BookDto> CreateAsync(CreateBookDto dto)
    {
        var authorIds = dto.AuthorIds.Distinct().ToList();
        await ValidateAsync(dto, authorIds, null);

        var book = new Book
        {
            Title = dto.Title.Trim(),
            ISBN = dto.ISBN.Trim(),
            PublishYear = dto.PublishYear,
            TotalCopies = dto.TotalCopies,
            AvailableCopies = dto.TotalCopies,       // a new book starts with every copy available
            CategoryId = dto.CategoryId
        };
        foreach (var authorId in authorIds)
            book.BookAuthors.Add(new BookAuthor { AuthorId = authorId });

        await _books.AddAsync(book);
        await _uow.SaveChangesAsync();
        return await GetByIdAsync(book.Id);
    }

    public async Task<BookDto> UpdateAsync(int id, UpdateBookDto dto)
    {
        var book = await _books.GetByIdAsync(id, track: true) ?? throw new NotFoundException("Book not found.");
        var authorIds = dto.AuthorIds.Distinct().ToList();
        await ValidateAsync(dto, authorIds, id);

        // copies currently on loan = old total - old available; the new total can't be lower than that
        var onLoan = book.TotalCopies - book.AvailableCopies;
        if (dto.TotalCopies < onLoan)
            throw new BusinessRuleException($"TotalCopies cannot be less than the {onLoan} copies currently on loan.");

        book.Title = dto.Title.Trim();
        book.ISBN = dto.ISBN.Trim();
        book.PublishYear = dto.PublishYear;
        book.TotalCopies = dto.TotalCopies;
        book.AvailableCopies = dto.TotalCopies - onLoan;
        book.CategoryId = dto.CategoryId;

        // only remove / add the differences (re-adding a deleted key in the same save would fail)
        var toRemove = book.BookAuthors.Where(x => !authorIds.Contains(x.AuthorId)).ToList();
        foreach (var link in toRemove) book.BookAuthors.Remove(link);
        foreach (var authorId in authorIds.Where(a => book.BookAuthors.All(x => x.AuthorId != a)))
            book.BookAuthors.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        var book = await _books.GetByIdAsync(id, track: true) ?? throw new NotFoundException("Book not found.");
        if (await _books.HasLoansAsync(id))
            throw new BusinessRuleException("This book has loan history and cannot be deleted.");
        _books.Remove(book);
        await _uow.SaveChangesAsync();
    }

    private async Task ValidateAsync(CreateBookDto dto, List<int> authorIds, int? excludeBookId)
    {
        if (await _categories.GetByIdAsync(dto.CategoryId) == null)
            throw new BusinessRuleException("The selected category does not exist.");

        var found = await _authors.GetByIdsAsync(authorIds);
        if (found.Count != authorIds.Count)
            throw new BusinessRuleException("One or more selected authors do not exist.");

        if (await _books.IsbnExistsAsync(dto.ISBN.Trim(), excludeBookId))
            throw new BusinessRuleException("A book with this ISBN already exists.");
    }
}
