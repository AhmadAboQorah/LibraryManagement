using Library.Application.DTOs;
using Library.Application.Exceptions;
using Library.Application.Interfaces;
using Library.Application.Mappers;
using Library.Domain.Entities;

namespace Library.Application.Services;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _authors;
    private readonly IUnitOfWork _uow;

    public AuthorService(IAuthorRepository authors, IUnitOfWork uow)
    {
        _authors = authors;
        _uow = uow;
    }

    public async Task<List<AuthorDto>> GetAllAsync() =>
        (await _authors.GetAllAsync()).Select(a => a.ToDto()).ToList();

    public async Task<AuthorDto> GetByIdAsync(int id) =>
        (await _authors.GetByIdAsync(id) ?? throw new NotFoundException("Author not found.")).ToDto();

    public async Task<AuthorDto> CreateAsync(SaveAuthorDto dto)
    {
        var author = new Author { Name = dto.Name.Trim(), Bio = dto.Bio?.Trim() };
        await _authors.AddAsync(author);
        await _uow.SaveChangesAsync();
        return author.ToDto();
    }

    public async Task<AuthorDto> UpdateAsync(int id, SaveAuthorDto dto)
    {
        var author = await _authors.GetByIdAsync(id) ?? throw new NotFoundException("Author not found.");
        author.Name = dto.Name.Trim();
        author.Bio = dto.Bio?.Trim();
        await _uow.SaveChangesAsync();
        return author.ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var author = await _authors.GetByIdAsync(id) ?? throw new NotFoundException("Author not found.");
        if (await _authors.HasBooksAsync(id))
            throw new BusinessRuleException("This author has books. Remove or change those books first.");
        _authors.Remove(author);
        await _uow.SaveChangesAsync();
    }
}

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _uow;

    public CategoryService(ICategoryRepository categories, IUnitOfWork uow)
    {
        _categories = categories;
        _uow = uow;
    }

    public async Task<List<CategoryDto>> GetAllAsync() =>
        (await _categories.GetAllAsync()).Select(c => c.ToDto()).ToList();

    public async Task<CategoryDto> GetByIdAsync(int id) =>
        (await _categories.GetByIdAsync(id) ?? throw new NotFoundException("Category not found.")).ToDto();

    public async Task<CategoryDto> CreateAsync(SaveCategoryDto dto)
    {
        var name = dto.Name.Trim();
        if (await _categories.NameExistsAsync(name))
            throw new BusinessRuleException("A category with this name already exists.");

        var category = new Category { Name = name };
        await _categories.AddAsync(category);
        await _uow.SaveChangesAsync();
        return category.ToDto();
    }

    public async Task<CategoryDto> UpdateAsync(int id, SaveCategoryDto dto)
    {
        var category = await _categories.GetByIdAsync(id) ?? throw new NotFoundException("Category not found.");
        var name = dto.Name.Trim();
        if (await _categories.NameExistsAsync(name, id))
            throw new BusinessRuleException("A category with this name already exists.");

        category.Name = name;
        await _uow.SaveChangesAsync();
        return category.ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _categories.GetByIdAsync(id) ?? throw new NotFoundException("Category not found.");
        if (await _categories.HasBooksAsync(id))
            throw new BusinessRuleException("This category contains books and cannot be deleted.");
        _categories.Remove(category);
        await _uow.SaveChangesAsync();
    }
}
