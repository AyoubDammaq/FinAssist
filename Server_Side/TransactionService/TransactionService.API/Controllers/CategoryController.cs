using MediatR;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Application.Commands.Category.AddCategory;
using TransactionService.Application.Commands.Category.UpdateCategory;
using TransactionService.Application.Commands.Category.DeleteCategory;
using TransactionService.Application.DTOs;
using TransactionService.Application.Queries.Category.GetCategoryById;
using TransactionService.Application.Queries.Category.GetCategoryByUser;
using TransactionService.Application.Queries.Category.GetAllCategories;

namespace TransactionService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CategoryController(IMediator mediator, ILogger<CategoryController> logger) : ControllerBase
    {
        private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        private readonly ILogger<CategoryController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] AddCategoryRequest addCategoryRequest, CancellationToken cancellationToken)
        {
            var validator = new AddCategoryCommandValidator();
            var validationResult = await validator.ValidateAsync(addCategoryRequest, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for AddCategoryRequest: {@Errors}", validationResult.Errors);
                return BadRequest(new { Message = "Validation failed.", validationResult.Errors });
            }

            var command = new AddCategoryCommand(addCategoryRequest);
            var result = await _mediator.Send(command, cancellationToken);
            if (result)
            {
                return Ok(new { Message = "Category added successfully." });
            }
            else
            {
                return BadRequest(new { Message = "Failed to add category. Please check the details and try again." });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryRequest updateCategoryRequest, CancellationToken cancellationToken)
        {
            var validator = new UpdateCategoryCommandValidator();
            var validationResult = await validator.ValidateAsync(updateCategoryRequest, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for UpdateCategoryRequest: {@Errors}", validationResult.Errors);
                return BadRequest(new { Message = "Validation failed.", validationResult.Errors });
            }

            var command = new UpdateCategoryCommand(updateCategoryRequest);
            var result = await _mediator.Send(command, cancellationToken);
            if (result)
            {
                return Ok(new { Message = "Category updated successfully." });
            }
            else
            {
                return BadRequest(new { Message = "Failed to update category. Please check the details and try again." });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCategory([FromQuery] Guid categoryId, CancellationToken cancellationToken)
        {
            if (categoryId == Guid.Empty)
            {
                _logger.LogWarning("Invalid category ID received in DeleteCategory request.");
                return BadRequest(new { Message = "Invalid category ID." });
            }
            var command = new DeleteCategoryCommand(categoryId);
            var result = await _mediator.Send(command, cancellationToken);
            if (result)
            {
                return Ok(new { Message = "Category deleted successfully." });
            }
            else
            {
                return NotFound(new { Message = "Category not found. Please check the ID and try again." });
            }
        }

        [HttpGet("ById")]
        public async Task<IActionResult> GetCategoryById([FromQuery] Guid categoryId, CancellationToken cancellationToken)
        {
            if (categoryId == Guid.Empty)
            {
                _logger.LogWarning("Invalid category ID received in GetCategoryById request.");
                return BadRequest(new { Message = "Invalid category ID." });
            }
            var query = new GetCategoryByIdQuery(categoryId);
            var category = await _mediator.Send(query, cancellationToken);
            if (category != null)
            {
                return Ok(category);
            }
            else
            {
                return NotFound(new { Message = "Category not found for the specified ID." });
            }
        }

        [HttpGet("ByUserId")]
        public async Task<IActionResult> GetCategoriesByUserId([FromQuery] Guid userId, CancellationToken cancellationToken)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Invalid user ID received in GetCategoriesByUserId request.");
                return BadRequest(new { Message = "Invalid user ID." });
            }
            var query = new GetCategoryByUserQuery(userId);
            var categories = await _mediator.Send(query, cancellationToken);
            if (categories != null && categories.Any())
            {
                return Ok(categories);
            }
            else
            {
                return NotFound(new { Message = "No categories found for the specified user ID." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories(CancellationToken cancellationToken)
        {
            var query = new GetAllCategoriesQuery();
            var categories = await _mediator.Send(query, cancellationToken);
            if (categories != null && categories.Any())
            {
                return Ok(categories);
            }
            else
            {
                return NotFound(new { Message = "No categories found." });
            }
        }
    }
}
