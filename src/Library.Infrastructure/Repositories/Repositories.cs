using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly LibraryDbContext _db;
    public UserRepository(LibraryDbContext db) => _db = db;

    public Task<User?> GetByIdAsync(int id) => _db.Users.FirstOrDefaultAsync(u => u.Id == id);
    public Task<User?> GetByEmailAsync(string email) => _db.Users.FirstOrDefaultAsync(u => u.Email == email);
    public Task<bool> EmailExistsAsync(string email) => _db.Users.AnyAsync(u => u.Email == email);
    public async Task AddAsync(User user) => await _db.Users.AddAsync(user);
}

public class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _db;
    public BookRepository(LibraryDbContext db) => _db = db;

    private IQueryable<Book> WithDetails() =>
        _db.Books.Include(b => b.Category).Include(b => b.BookAuthors).ThenInclude(ba => ba.Author);

    public Task<List<Book>> SearchAsync(string? title, int? categoryId)
    {
        var query = WithDetails().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(b => b.Title.Contains(title.Trim()));

        if (categoryId.HasValue)
            query = query.Where(b => b.CategoryId == categoryId.Value);

        return query.OrderBy(b => b.Title).ToListAsync();
    }

    public Task<Book?> GetByIdAsync(int id, bool track = false) =>
        (track ? WithDetails() : WithDetails().AsNoTracking()).FirstOrDefaultAsync(b => b.Id == id);

    public Task<bool> IsbnExistsAsync(string isbn, int? excludeBookId = null) =>
        _db.Books.AnyAsync(b => b.ISBN == isbn && (excludeBookId == null || b.Id != excludeBookId));

    public Task<bool> HasLoansAsync(int bookId) => _db.Loans.AnyAsync(l => l.BookId == bookId);
    public async Task AddAsync(Book book) => await _db.Books.AddAsync(book);
    public void Remove(Book book) => _db.Books.Remove(book);
}

public class AuthorRepository : IAuthorRepository
{
    private readonly LibraryDbContext _db;
    public AuthorRepository(LibraryDbContext db) => _db = db;

    public Task<List<Author>> GetAllAsync() => _db.Authors.AsNoTracking().OrderBy(a => a.Name).ToListAsync();
    public Task<Author?> GetByIdAsync(int id) => _db.Authors.FirstOrDefaultAsync(a => a.Id == id);
    public Task<List<Author>> GetByIdsAsync(IEnumerable<int> ids) =>
        _db.Authors.Where(a => ids.Contains(a.Id)).ToListAsync();
    public Task<bool> HasBooksAsync(int authorId) => _db.BookAuthors.AnyAsync(x => x.AuthorId == authorId);
    public async Task AddAsync(Author author) => await _db.Authors.AddAsync(author);
    public void Remove(Author author) => _db.Authors.Remove(author);
}

public class CategoryRepository : ICategoryRepository
{
    private readonly LibraryDbContext _db;
    public CategoryRepository(LibraryDbContext db) => _db = db;

    public Task<List<Category>> GetAllAsync() => _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
    public Task<Category?> GetByIdAsync(int id) => _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
    public Task<bool> NameExistsAsync(string name, int? excludeId = null) =>
        _db.Categories.AnyAsync(c => c.Name == name && (excludeId == null || c.Id != excludeId));
    public Task<bool> HasBooksAsync(int categoryId) => _db.Books.AnyAsync(b => b.CategoryId == categoryId);
    public async Task AddAsync(Category category) => await _db.Categories.AddAsync(category);
    public void Remove(Category category) => _db.Categories.Remove(category);
}

public class LoanRepository : ILoanRepository
{
    private readonly LibraryDbContext _db;
    public LoanRepository(LibraryDbContext db) => _db = db;

    public Task<Loan?> GetByIdAsync(int id) =>
        _db.Loans.Include(l => l.Book).Include(l => l.User).FirstOrDefaultAsync(l => l.Id == id);

    public Task<List<Loan>> GetByUserAsync(int userId) =>
        _db.Loans.AsNoTracking().Include(l => l.Book).Include(l => l.User)
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.LoanDate).ToListAsync();

    public Task<List<Loan>> GetOverdueAsync(DateTime nowUtc) =>
        _db.Loans.AsNoTracking().Include(l => l.Book).Include(l => l.User)
            .Where(l => l.ReturnDate == null && l.DueDate < nowUtc)
            .OrderBy(l => l.DueDate).ToListAsync();

    public Task<int> CountActiveByUserAsync(int userId) =>
        _db.Loans.CountAsync(l => l.UserId == userId && l.ReturnDate == null);

    public Task<bool> HasActiveLoanForBookAsync(int userId, int bookId) =>
        _db.Loans.AnyAsync(l => l.UserId == userId && l.BookId == bookId && l.ReturnDate == null);

    public async Task AddAsync(Loan loan) => await _db.Loans.AddAsync(loan);
}
