using Library.Domain.Enums;

namespace Library.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;

    // MIGRATION-2 (step 1 of 4): uncomment this line, then see README "Second migration"
    // public string? PhoneNumber { get; set; }

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
