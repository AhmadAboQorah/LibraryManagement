using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs;

public class CreateLoanDto
{
    [Range(1, int.MaxValue, ErrorMessage = "BookId is required")]
    public int BookId { get; set; }
}

public class LoanDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public bool IsOverdue { get; set; }
    public string Status { get; set; } = string.Empty;   // Active | Overdue | Returned
}
