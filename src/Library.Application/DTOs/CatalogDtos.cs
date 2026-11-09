using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs;

// ---------- Authors ----------
public class AuthorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }
}

public class SaveAuthorDto
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Bio { get; set; }
}

// ---------- Categories ----------
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class SaveCategoryDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
}

// ---------- Books ----------
public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int PublishYear { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<AuthorDto> Authors { get; set; } = new();
}

public class CreateBookDto
{
    [Required, StringLength(250, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(20, MinimumLength = 10)]
    public string ISBN { get; set; } = string.Empty;

    [Range(1000, 2100)]
    public int PublishYear { get; set; }

    [Range(1, 10000)]
    public int TotalCopies { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Required, MinLength(1, ErrorMessage = "A book needs at least one author")]
    public List<int> AuthorIds { get; set; } = new();
}

public class UpdateBookDto : CreateBookDto { }
