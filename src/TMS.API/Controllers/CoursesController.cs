using MediatR;
using Microsoft.AspNetCore.Mvc;
using TMS.Application.Courses.Commands.CreateCourse;
using TMS.Application.Courses.DTOs;
using TMS.Domain.Exceptions;
using TMS.Domain.Repositories;

namespace TMS.API.Controllers;

/// <summary>
/// Manages courses — create and retrieve by ID.
/// </summary>
[ApiController]
[Route("api/courses")]
[Produces("application/json")]
public sealed class CoursesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICourseRepository _courseRepository;

    public CoursesController(IMediator mediator, ICourseRepository courseRepository)
    {
        _mediator = mediator;
        _courseRepository = courseRepository;
    }

    /// <summary>Creates a new course linked to an existing subject.</summary>
    /// <response code="201">Returns the newly created course.</response>
    /// <response code="400">Validation error — name is missing/exceeds 200 chars, or SubjectId is empty.</response>
    /// <response code="404">The referenced subject does not exist.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CourseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateCourse(
        [FromBody] CreateCourseCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
    }

    /// <summary>Returns a course by its ID.</summary>
    /// <response code="200">Course found.</response>
    /// <response code="404">Course not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseById(Guid id, CancellationToken ct)
    {
        var course = await _courseRepository.GetByIdAsync(id, ct);
        if (course is null)
            throw new NotFoundException($"Course with ID '{id}' was not found.");

        var dto = new CourseDto(
            course.Id,
            course.Name,
            course.Description,
            course.SubjectId,
            course.CreatedAt,
            course.UpdatedAt);

        return Ok(dto);
    }
}
