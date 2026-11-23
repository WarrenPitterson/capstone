using System.Security.Claims;
using BookWorm.Api.Models;
using BookWorm.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedBooksResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedBooksResponse>> GetBooks([FromQuery] BookQueryParameters parameters)
    {
        var response = await _bookService.GetBooksAsync(parameters);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookDetailDto>> GetBookById(int id)
    {
        var book = await _bookService.GetBookByIdAsync(id);
        return Ok(book);
    }

    [HttpGet("recommended")]
    [ProducesResponseType(typeof(List<BookSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BookSummaryDto>>> GetRecommended()
    {
        int? userId = null;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var books = await _bookService.GetRecommendedBooksAsync(userId);
        return Ok(books);
    }

    [HttpGet("bestsellers")]
    [ProducesResponseType(typeof(List<BookSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BookSummaryDto>>> GetBestsellers()
    {
        var books = await _bookService.GetBestsellersAsync();
        return Ok(books);
    }

    [HttpGet("new-launches")]
    [ProducesResponseType(typeof(List<BookSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BookSummaryDto>>> GetNewLaunches()
    {
        var books = await _bookService.GetNewLaunchesAsync();
        return Ok(books);
    }
}
