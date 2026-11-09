using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.FullName).IsRequired().HasMaxLength(100);
        b.Property(x => x.Email).IsRequired().HasMaxLength(150);
        b.HasIndex(x => x.Email).IsUnique();                 // one account per email
        b.Property(x => x.PasswordHash).IsRequired().HasMaxLength(200);
        b.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);   // stored as "Admin"/"User"
    }
}

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(150);
        b.Property(x => x.Bio).HasMaxLength(1000);
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(100);
        b.HasIndex(x => x.Name).IsUnique();
    }
}

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).IsRequired().HasMaxLength(250);
        b.Property(x => x.ISBN).IsRequired().HasMaxLength(20);
        b.HasIndex(x => x.ISBN).IsUnique();
        b.Property(x => x.RowVersion).IsRowVersion();

        // DB-level safety net for the copies counters
        b.ToTable(t => t.HasCheckConstraint("CK_Books_Copies",
            "[AvailableCopies] >= 0 AND [AvailableCopies] <= [TotalCopies]"));

        // One-to-Many: Category -> Books (can't delete a category that still has books)
        b.HasOne(x => x.Category)
         .WithMany(c => c.Books)
         .HasForeignKey(x => x.CategoryId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BookAuthorConfiguration : IEntityTypeConfiguration<BookAuthor>
{
    public void Configure(EntityTypeBuilder<BookAuthor> b)
    {
        // Many-to-Many via the join table: composite primary key (BookId, AuthorId)
        b.HasKey(x => new { x.BookId, x.AuthorId });

        b.HasOne(x => x.Book).WithMany(x => x.BookAuthors)
         .HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Author).WithMany(x => x.BookAuthors)
         .HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> b)
    {
        b.HasKey(x => x.Id);
        b.Ignore(x => x.IsActive);   // computed, not a column

        b.HasOne(x => x.Book).WithMany(x => x.Loans)
         .HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.User).WithMany(x => x.Loans)
         .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.UserId, x.ReturnDate });   // speeds up "my active loans"
    }
}
