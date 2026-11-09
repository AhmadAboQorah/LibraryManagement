using Library.Application.DTOs;
using Library.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly IBookService _books;
    public BooksController(IBookService books) => _books = books;

    /// <summary>List books. Optional search by title and filter by category.</summary>
    [HttpGet]
    public async Task<ActionResult<List<BookDto>>> GetAll([FromQuery] string? title, [FromQuery] int? categoryId) =>
        Ok(await _books.SearchAsync(title, categoryId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDto>> GetById(int id) => Ok(await _books.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BookDto>> Create(CreateBookDto dto)
    {
        var book = await _books.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BookDto>> Update(int id, UpdateBookDto dto) => Ok(await _books.UpdateAsync(id, dto));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _books.DeleteAsync(id);
        return NoContent();
    }
}
