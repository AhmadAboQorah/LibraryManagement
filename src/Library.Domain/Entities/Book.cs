namespace Library.Domain.Entities;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int PublishYear { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }

    // Used for optimistic concurrency: two people borrowing the last copy at the same time
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
