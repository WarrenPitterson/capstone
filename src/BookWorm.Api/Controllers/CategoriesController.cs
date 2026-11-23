using BookWorm.Api.Models;
using BookWorm.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IBookService _bookService;

    public CategoriesController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories()
    {
        var categories = await _bookService.GetCategoriesAsync();
        return Ok(categories);
    }
}
