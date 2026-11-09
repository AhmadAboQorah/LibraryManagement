using Library.Application.DTOs;
using Library.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers;

[ApiController]
[Route("api/authors")]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService _authors;
    public AuthorsController(IAuthorService authors) => _authors = authors;

    [HttpGet]
    public async Task<ActionResult<List<AuthorDto>>> GetAll() => Ok(await _authors.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AuthorDto>> GetById(int id) => Ok(await _authors.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AuthorDto>> Create(SaveAuthorDto dto)
    {
        var author = await _authors.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = author.Id }, author);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AuthorDto>> Update(int id, SaveAuthorDto dto) => Ok(await _authors.UpdateAsync(id, dto));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _authors.DeleteAsync(id);
        return NoContent();
    }
}
