namespace Library.Domain.Entities;

public class Loan
{
    public const int LoanDays = 14;
    public const int MaxActiveLoansPerUser = 3;

    public int Id { get; set; }
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }

    public bool IsActive => ReturnDate == null;

    // A loan is overdue when it passed DueDate and was not returned
    public bool IsOverdue(DateTime nowUtc) => ReturnDate == null && nowUtc > DueDate;
}
