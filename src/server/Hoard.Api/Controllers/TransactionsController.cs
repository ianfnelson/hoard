using FluentValidation;
using Hoard.Core.Application;
using Hoard.Core.Application.Transactions;
using Hoard.Core.Application.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/transactions/")]
[Produces("application/json")]
[Tags("Transactions")]
public class TransactionsController(IMediator mediator, IValidator<TransactionWriteDto> validator) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(TransactionDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TransactionDetailDto>> Get(int id, CancellationToken ct)
    {
        var query = new GetTransactionQuery(id);
        var dto = await mediator.QueryAsync<GetTransactionQuery, TransactionDetailDto?>(query, ct);
        return dto == null ? new NotFoundResult() : new OkObjectResult(dto);
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TransactionSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TransactionSummaryDto>>> GetList([FromQuery] GetTransactionsQuery query, CancellationToken ct)
    {
        var dtos = await mediator.QueryAsync<GetTransactionsQuery, PagedResult<TransactionSummaryDto>>(query, ct);
        return new OkObjectResult(dtos);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> Create([FromBody] TransactionWriteDto request, CancellationToken ct)
    {
        var problems = await validator.ValidateAndGetProblemsAsync(request, cancellationToken: ct);
        if (problems != null)
        {
            return BadRequest(problems);
        }

        var id = await mediator.SendAsync<CreateTransactionCommand, int>(new CreateTransactionCommand(request), ct);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }
    
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Update(int id, [FromBody] TransactionWriteDto request, CancellationToken ct)
    {
        var problems = await validator.ValidateAndGetProblemsAsync(request, entityId: id, cancellationToken: ct);
        if (problems != null)
        {
            return BadRequest(problems);
        }

        await mediator.SendAsync(new UpdateTransactionCommand(id, request), ct);
        return NoContent();
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var command = new DeleteTransactionCommand(id);
        await mediator.SendAsync(command, ct);
        return NoContent();  // 204
    }

    [HttpPost("{id:int}/contractnote")]
    [ProducesResponseType(typeof(ContractNoteUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ContractNoteUploadResponse>> UploadContractNote(
        int id, IFormFile file, CancellationToken ct)
    {
        // API-level validations
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only PDF files are allowed");

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest("File size exceeds 10MB limit");

        var reference = ExtractReferenceFromFilename(file.FileName);
        if (string.IsNullOrWhiteSpace(reference) || reference.Length > 20)
            return BadRequest("Invalid filename format");

        // Delegate to command
        try
        {
            await using var stream = file.OpenReadStream();
            var command = new UploadContractNoteCommand(id, reference, stream);
            var blobUri = await mediator.SendAsync<UploadContractNoteCommand, string>(command, ct);

            return Ok(new ContractNoteUploadResponse(reference, blobUri));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{id:int}/contractnote")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadContractNote(int id, CancellationToken ct)
    {
        try
        {
            var query = new GetContractNoteQuery(id);
            var result = await mediator.QueryAsync<GetContractNoteQuery, ContractNoteResult>(query, ct);

            return File(result.Stream, result.ContentType, result.FileName);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return NotFound("Contract note file not found in storage");
        }
    }

    [HttpDelete("{id:int}/contractnote")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContractNote(int id, CancellationToken ct)
    {
        try
        {
            var command = new DeleteContractNoteCommand(id);
            await mediator.SendAsync(command, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    private static string ExtractReferenceFromFilename(string filename)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
        var underscoreIndex = nameWithoutExtension.IndexOf('_');
        return underscoreIndex > 0
            ? nameWithoutExtension[..underscoreIndex]
            : nameWithoutExtension;
    }
}

public record ContractNoteUploadResponse(string Reference, string BlobUri);