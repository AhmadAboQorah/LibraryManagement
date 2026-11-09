namespace Library.Domain.Entities;

// Join entity for the many-to-many relation Book <-> Author
public class BookAuthor
{
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;

    public int AuthorId { get; set; }
    public Author Author { get; set; } = null!;
}
