using MediatR;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Application.Commands.Transaction.AddTransaction;
using TransactionService.Application.Commands.Transaction.DeleteTransaction;
using TransactionService.Application.Commands.Transaction.UpdateTransaction;
using TransactionService.Application.DTOs;
using TransactionService.Application.Queries.Transaction.GetTransactionById;
using TransactionService.Application.Queries.Transaction.GetTransactionsByCategory;
using TransactionService.Application.Queries.Transaction.GetTransactionsByDate;
using TransactionService.Application.Queries.Transaction.GetTransactionsByUser;

namespace TransactionService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]

    public class TransactionController(IMediator mediator, ILogger<TransactionController> logger) : ControllerBase
    {
        private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        private readonly ILogger<TransactionController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [HttpPost]
        public async Task<IActionResult> AddTransaction([FromBody] AddTransactionRequest addTransactionRequest, CancellationToken cancellationToken)
        {
            var validator = new AddTransactionCommandValidator();
            var validationResult = await validator.ValidateAsync(addTransactionRequest, cancellationToken);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for AddTransactionRequest: {@Errors}", validationResult.Errors);
                return BadRequest(new { Message = "Validation failed.", validationResult.Errors });
            }

            var command = new AddTransactionCommand(addTransactionRequest);
            var result = await _mediator.Send(command, cancellationToken);
            if (result)
            {
                return Ok(new { Message = "Transaction added successfully." });
            }
            else
            {
                return BadRequest(new { Message = "Failed to add transaction. Please check the details and try again." });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTransaction([FromBody] UpdateTransactionRequest updateTransactionRequest, CancellationToken cancellationToken)
        {
            var validator = new UpdateTransactionCommandValidator();
            var validationResult = await validator.ValidateAsync(updateTransactionRequest, cancellationToken);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for UpdateTransactionRequest: {@Errors}", validationResult.Errors);
                return BadRequest(new { Message = "Validation failed.", validationResult.Errors });
            }

            var command = new UpdateTransactionCommand(updateTransactionRequest);
            var result = await _mediator.Send(command, cancellationToken);
            if (result)
            {
                return Ok(new { Message = "Transaction updated successfully." });
            }
            else
            {
                return BadRequest(new { Message = "Failed to update transaction. Please check the details and try again." });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTransaction([FromQuery] Guid transactionId, CancellationToken cancellationToken)
        {

            if (transactionId == Guid.Empty)
            {
                _logger.LogWarning("Invalid transaction ID received in DeleteTransaction request.");
                return BadRequest(new { Message = "Invalid transaction ID." });
            }
            var command = new DeleteTransactionCommand(transactionId);
            var result = await _mediator.Send(command, cancellationToken);
            if (result)
            {
                return Ok(new { Message = "Transaction deleted successfully." });
            }
            else
            {
                return NotFound(new { Message = "Transaction not found. Please check the ID and try again." });
            }
        }

        [HttpGet("ById")]
        public async Task<IActionResult> GetTransactionById([FromQuery] Guid transactionId, CancellationToken cancellationToken)
        {

            if (transactionId == Guid.Empty)
            {
                _logger.LogWarning("Invalid transaction ID received in GetTransactionById request.");
                return BadRequest(new { Message = "Invalid transaction ID." });
            }
            var query = new GetTransactionByIdQuery(transactionId);
            var transaction = await _mediator.Send(query, cancellationToken);
            if (transaction != null)
            {
                return Ok(transaction);
            }
            else
            {
                return NotFound(new { Message = "Transaction not found for the specified ID." });
            }
        }

        [HttpGet("ByUserId")]
        public async Task<IActionResult> GetTransactionsByUserId([FromQuery] Guid userId, CancellationToken cancellationToken)
        {

            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Invalid user ID received in GetTransactionsByUserId request.");
                return BadRequest(new { Message = "Invalid user ID." });
            }
            var query = new GetTransactionsByUserQuery(userId);
            var transactions = await _mediator.Send(query, cancellationToken);
            if (transactions != null && transactions.Any())
            {
                return Ok(transactions);
            }
            else
            {
                return NotFound(new { Message = "No transactions found for the specified user ID." });
            }
        }

        [HttpGet("ByCategoryId")]
        public async Task<IActionResult> GetTransactionsByCategoryId([FromQuery] Guid categoryId, [FromQuery] Guid userId, CancellationToken cancellationToken)
        {

            if (categoryId == Guid.Empty)
            {
                _logger.LogWarning("Invalid category ID received in GetTransactionsByCategoryId request.");
                return BadRequest(new { Message = "Invalid category ID." });
            }
            var query = new GetTransactionsByCategoryQuery(userId, categoryId);
            var transactions = await _mediator.Send(query, cancellationToken);
            if (transactions != null && transactions.Any())
            {
                return Ok(transactions);
            }
            else
            {
                return NotFound(new { Message = "No transactions found for the specified category ID." });
            }
        }

        [HttpGet("ByDate")]
        public async Task<IActionResult> GetTransactionsByDate([FromQuery] DateTime date, CancellationToken cancellationToken)
        {

            if (date == default)
            {
                _logger.LogWarning("Invalid date received in GetTransactionsByDate request.");
                return BadRequest(new { Message = "Invalid date." });
            }
            var query = new GetTransactionsByDateQuery(date);
            var transactions = await _mediator.Send(query, cancellationToken);
            if (transactions != null && transactions.Any())
            {
                return Ok(transactions);
            }
            else
            {
                return NotFound(new { Message = "No transactions found for the specified date." });
            }
        }
    }
}