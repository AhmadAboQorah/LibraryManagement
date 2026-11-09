using Library.API.Extensions;
using Library.Application.DTOs;
using Library.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers;

[ApiController]
[Route("api/loans")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loans;
    public LoansController(ILoanService loans) => _loans = loans;

    /// <summary>Borrow a book (rules: copies available, max 3 active loans, 14 days).</summary>
    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<LoanDto>> Borrow(CreateLoanDto dto)
    {
        var loan = await _loans.BorrowAsync(User.GetUserId(), dto.BookId);
        return StatusCode(StatusCodes.Status201Created, loan);
    }

    /// <summary>Return one of my books.</summary>
    [HttpPut("{id:int}/return")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<LoanDto>> Return(int id) => Ok(await _loans.ReturnAsync(User.GetUserId(), id));

    /// <summary>My current and past loans.</summary>
    [HttpGet("my")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<List<LoanDto>>> My() => Ok(await _loans.GetMyLoansAsync(User.GetUserId()));

    /// <summary>All overdue loans (Admin).</summary>
    [HttpGet("overdue")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<LoanDto>>> Overdue() => Ok(await _loans.GetOverdueAsync());
}
