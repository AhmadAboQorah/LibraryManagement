using Library.Application.DTOs;
using Library.Application.Exceptions;
using Library.Application.Interfaces;
using Library.Application.Mappers;
using Library.Domain.Entities;

namespace Library.Application.Services;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loans;
    private readonly IBookRepository _books;
    private readonly IUnitOfWork _uow;

    public LoanService(ILoanRepository loans, IBookRepository books, IUnitOfWork uow)
    {
        _loans = loans;
        _books = books;
        _uow = uow;
    }

    public async Task<LoanDto> BorrowAsync(int userId, int bookId)
    {
        var book = await _books.GetByIdAsync(bookId, track: true) ?? throw new NotFoundException("Book not found.");

        // Business rule 1: no copies left
        if (book.AvailableCopies <= 0)
            throw new BusinessRuleException("This book is not available right now (no copies left).");

        // Business rule 2: max 3 active loans per member
        if (await _loans.CountActiveByUserAsync(userId) >= Loan.MaxActiveLoansPerUser)
            throw new BusinessRuleException($"You cannot borrow more than {Loan.MaxActiveLoansPerUser} books at the same time.");

        // Extra rule: the same member can't hold two copies of the same book
        if (await _loans.HasActiveLoanForBookAsync(userId, bookId))
            throw new BusinessRuleException("You already have an active loan for this book.");

        var now = DateTime.UtcNow;
        var loan = new Loan
        {
            BookId = bookId,
            UserId = userId,
            LoanDate = now,
            DueDate = now.AddDays(Loan.LoanDays),   // 14 days
            Book = book
        };
        book.AvailableCopies--;

        await _loans.AddAsync(loan);
        await _uow.SaveChangesAsync();   // RowVersion on Book protects against two people taking the last copy
        return loan.ToDto(now);
    }

    public async Task<LoanDto> ReturnAsync(int userId, int loanId)
    {
        var loan = await _loans.GetByIdAsync(loanId) ?? throw new NotFoundException("Loan not found.");

        if (loan.UserId != userId)
            throw new ForbiddenException("You can only return your own loans.");

        if (loan.ReturnDate != null)
            throw new BusinessRuleException("This book was already returned.");

        var now = DateTime.UtcNow;
        loan.ReturnDate = now;
        loan.Book.AvailableCopies++;

        await _uow.SaveChangesAsync();
        return loan.ToDto(now);
    }

    public async Task<List<LoanDto>> GetMyLoansAsync(int userId)
    {
        var now = DateTime.UtcNow;
        return (await _loans.GetByUserAsync(userId)).Select(l => l.ToDto(now)).ToList();
    }

    public async Task<List<LoanDto>> GetOverdueAsync()
    {
        var now = DateTime.UtcNow;
        return (await _loans.GetOverdueAsync(now)).Select(l => l.ToDto(now)).ToList();
    }
}
