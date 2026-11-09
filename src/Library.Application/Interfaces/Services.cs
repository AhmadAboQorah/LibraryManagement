using Library.Application.DTOs;
using Library.Domain.Entities;

namespace Library.Application.Interfaces;

// Infrastructure implements these two (BCrypt and JWT are libraries, so they don't belong in Application)
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}

public interface IBookService
{
    Task<List<BookDto>> SearchAsync(string? title, int? categoryId);
    Task<BookDto> GetByIdAsync(int id);
    Task<BookDto> CreateAsync(CreateBookDto dto);
    Task<BookDto> UpdateAsync(int id, UpdateBookDto dto);
    Task DeleteAsync(int id);
}

public interface IAuthorService
{
    Task<List<AuthorDto>> GetAllAsync();
    Task<AuthorDto> GetByIdAsync(int id);
    Task<AuthorDto> CreateAsync(SaveAuthorDto dto);
    Task<AuthorDto> UpdateAsync(int id, SaveAuthorDto dto);
    Task DeleteAsync(int id);
}

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto> GetByIdAsync(int id);
    Task<CategoryDto> CreateAsync(SaveCategoryDto dto);
    Task<CategoryDto> UpdateAsync(int id, SaveCategoryDto dto);
    Task DeleteAsync(int id);
}

public interface ILoanService
{
    Task<LoanDto> BorrowAsync(int userId, int bookId);
    Task<LoanDto> ReturnAsync(int userId, int loanId);
    Task<List<LoanDto>> GetMyLoansAsync(int userId);
    Task<List<LoanDto>> GetOverdueAsync();
}
