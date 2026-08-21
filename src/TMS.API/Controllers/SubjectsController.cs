using MediatR;
using Microsoft.AspNetCore.Mvc;
using TMS.Application.Subjects.Commands.CreateSubject;
using TMS.Application.Subjects.Commands.DeleteSubject;
using TMS.Application.Subjects.DTOs;
using TMS.Application.Subjects.Queries.GetAllSubjects;

namespace TMS.API.Controllers;

/// <summary>
/// Manages academic subjects — create, list, and soft-delete.
/// </summary>
[ApiController]
[Route("api/subjects")]
[Produces("application/json")]
public sealed class SubjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Creates a new subject.</summary>
    /// <response code="201">Returns the newly created subject.</response>
    /// <response code="400">Validation error — name is missing or exceeds 200 characters.</response>
    /// <response code="409">A subject with that name already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SubjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateSubject(
        [FromBody] CreateSubjectCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetAllSubjects), new { }, result);
    }

    /// <summary>Returns all active (non-deleted) subjects.</summary>
    /// <response code="200">List of subjects.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SubjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSubjects(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllSubjectsQuery(), ct);
        return Ok(result);
    }

    /// <summary>Soft-deletes a subject by ID.</summary>
    /// <response code="204">Subject deleted successfully.</response>
    /// <response code="404">Subject not found.</response>
    /// <response code="422">Subject is currently assigned to at least one teacher.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteSubject(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteSubjectCommand(id), ct);
        return NoContent();
    }
}
