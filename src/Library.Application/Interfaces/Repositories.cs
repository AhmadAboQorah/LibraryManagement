using Library.Domain.Entities;

namespace Library.Application.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task AddAsync(User user);
}

public interface IBookRepository
{
    Task<List<Book>> SearchAsync(string? title, int? categoryId);
    Task<Book?> GetByIdAsync(int id, bool track = false);
    Task<bool> IsbnExistsAsync(string isbn, int? excludeBookId = null);
    Task<bool> HasLoansAsync(int bookId);
    Task AddAsync(Book book);
    void Remove(Book book);
}

public interface IAuthorRepository
{
    Task<List<Author>> GetAllAsync();
    Task<Author?> GetByIdAsync(int id);
    Task<List<Author>> GetByIdsAsync(IEnumerable<int> ids);
    Task<bool> HasBooksAsync(int authorId);
    Task AddAsync(Author author);
    void Remove(Author author);
}

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
    Task<bool> HasBooksAsync(int categoryId);
    Task AddAsync(Category category);
    void Remove(Category category);
}

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(int id);                       // tracked, includes Book + User
    Task<List<Loan>> GetByUserAsync(int userId);
    Task<List<Loan>> GetOverdueAsync(DateTime nowUtc);
    Task<int> CountActiveByUserAsync(int userId);
    Task<bool> HasActiveLoanForBookAsync(int userId, int bookId);
    Task AddAsync(Loan loan);
}
