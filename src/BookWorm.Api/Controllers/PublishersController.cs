using BookWorm.Api.Models;
using BookWorm.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublishersController : ControllerBase
{
    private readonly IBookService _bookService;

    public PublishersController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<PublisherDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PublisherDto>>> GetPublishers()
    {
        var publishers = await _bookService.GetPublishersAsync();
        return Ok(publishers);
    }
}
