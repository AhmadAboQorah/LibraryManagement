using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Library.Infrastructure.Persistence;

/// <summary>
/// Seeds an Admin, a demo User and sample books. Done in code (not HasData) because the
/// password must be hashed with BCrypt, and BCrypt gives a different hash every time.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(LibraryDbContext db, IPasswordHasher hasher, IConfiguration config)
    {
        if (!await db.Users.AnyAsync())
        {
            db.Users.Add(new User
            {
                FullName = "Library Admin",
                Email = (config["SeedAdmin:Email"] ?? "admin@library.com").ToLowerInvariant(),
                PasswordHash = hasher.Hash(config["SeedAdmin:Password"] ?? "Admin@123"),
                Role = UserRole.Admin
            });
            db.Users.Add(new User
            {
                FullName = "Demo Member",
                Email = "user@library.com",
                PasswordHash = hasher.Hash("User@123"),
                Role = UserRole.User
            });
            await db.SaveChangesAsync();
        }

        if (await db.Books.AnyAsync()) return;

        var programming = new Category { Name = "Programming" };
        var fiction = new Category { Name = "Fiction" };
        var science = new Category { Name = "Science" };

        var martin = new Author { Name = "Robert C. Martin", Bio = "Author of Clean Code and Clean Architecture." };
        var skeet = new Author { Name = "Jon Skeet", Bio = "Author of C# in Depth." };
        var orwell = new Author { Name = "George Orwell", Bio = "English novelist and essayist." };
        var asimov = new Author { Name = "Isaac Asimov", Bio = "Science fiction writer and biochemist." };

        db.Books.AddRange(
            NewBook("Clean Code", "9780132350884", 2008, 3, programming, martin),
            NewBook("Clean Architecture", "9780134494166", 2017, 2, programming, martin),
            NewBook("C# in Depth", "9781617294532", 2019, 2, programming, skeet),
            NewBook("1984", "9780451524935", 1949, 4, fiction, orwell),
            NewBook("Animal Farm", "9780451526342", 1945, 1, fiction, orwell),
            NewBook("I, Robot", "9780553382563", 1950, 2, science, asimov),
            NewBook("Foundation", "9780553293357", 1951, 3, science, asimov));

        await db.SaveChangesAsync();
    }

    private static Book NewBook(string title, string isbn, int year, int copies, Category category, Author author)
    {
        var book = new Book
        {
            Title = title, ISBN = isbn, PublishYear = year,
            TotalCopies = copies, AvailableCopies = copies, Category = category
        };
        book.BookAuthors.Add(new BookAuthor { Author = author });
        return book;
    }
}
