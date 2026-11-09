using Library.Application.DTOs;
using Library.Domain.Entities;

namespace Library.Application.Mappers;

public static class Mappers
{
    public static UserDto ToDto(this User u) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Email = u.Email,
        Role = u.Role.ToString()
        // MIGRATION-2 (step 4 of 4): uncomment the next line (it starts with a comma)
        // , PhoneNumber = u.PhoneNumber
    };

    public static AuthorDto ToDto(this Author a) => new() { Id = a.Id, Name = a.Name, Bio = a.Bio };

    public static CategoryDto ToDto(this Category c) => new() { Id = c.Id, Name = c.Name };

    public static BookDto ToDto(this Book b) => new()
    {
        Id = b.Id,
        Title = b.Title,
        ISBN = b.ISBN,
        PublishYear = b.PublishYear,
        TotalCopies = b.TotalCopies,
        AvailableCopies = b.AvailableCopies,
        CategoryId = b.CategoryId,
        CategoryName = b.Category?.Name ?? string.Empty,
        Authors = b.BookAuthors.Where(x => x.Author != null).Select(x => x.Author.ToDto()).ToList()
    };

    public static LoanDto ToDto(this Loan l, DateTime nowUtc)
    {
        var overdue = l.IsOverdue(nowUtc);
        return new LoanDto
        {
            Id = l.Id,
            BookId = l.BookId,
            BookTitle = l.Book?.Title ?? string.Empty,
            UserId = l.UserId,
            UserName = l.User?.FullName ?? string.Empty,
            LoanDate = l.LoanDate,
            DueDate = l.DueDate,
            ReturnDate = l.ReturnDate,
            IsOverdue = overdue,
            Status = l.ReturnDate != null ? "Returned" : overdue ? "Overdue" : "Active"
        };
    }
}
