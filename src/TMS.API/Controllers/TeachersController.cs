using MediatR;
using Microsoft.AspNetCore.Mvc;
using TMS.Application.Common;
using TMS.Application.Teachers.Commands.AssignSubjectToTeacher;
using TMS.Application.Teachers.Commands.AssignTeacherToCourse;
using TMS.Application.Teachers.Commands.CreateTeacher;
using TMS.Application.Teachers.Commands.DeleteTeacher;
using TMS.Application.Teachers.Commands.RemoveSubjectFromTeacher;
using TMS.Application.Teachers.Commands.RemoveTeacherFromCourse;
using TMS.Application.Teachers.Commands.SetTeacherAvailability;
using TMS.Application.Teachers.Commands.UpdateTeacher;
using TMS.Application.Teachers.DTOs;
using TMS.Application.Teachers.Queries.GetAllTeachers;
using TMS.Application.Teachers.Queries.GetAvailableTeachers;
using TMS.Application.Teachers.Queries.GetSubjectsByTeacher;
using TMS.Application.Teachers.Queries.GetTeacherAvailability;
using TMS.Application.Teachers.Queries.GetTeacherById;
using TMS.Application.Teachers.Queries.GetTeacherSchedule;
using TMS.Application.Subjects.DTOs;

namespace TMS.API.Controllers;

/// <summary>
/// Request body for assigning a subject to a teacher.
/// </summary>

// The controller is therefore the entry point of the API
public record AssignSubjectRequest(Guid SubjectId);

/// <summary>
/// Request body for assigning a teacher to a course.
/// </summary>
public record AssignToCourseRequest(
    Guid CourseId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime);

/// <summary>
/// Manages teacher profiles and all teacher-related sub-resources:
/// subjects, availability slots, and course schedule entries.
/// </summary>
[ApiController]
[Route("api/teachers")]
[Produces("application/json")]
public sealed class TeachersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TeachersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ── Teacher CRUD ──────────────────────────────────────────────────────────

    /// <summary>Creates a new teacher profile.</summary>
    /// <response code="201">Returns the newly created teacher profile.</response>
    /// <response code="400">Validation error — missing or invalid fields.</response>
    /// <response code="409">A teacher with that email already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(TeacherDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateTeacher(
        [FromBody] CreateTeacherCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetTeacherById), new { id = result.Id }, result);
    }

    /// <summary>Returns the full profile of a specific teacher.</summary>
    /// <response code="200">Teacher profile found.</response>
    /// <response code="404">Teacher not found or has been deleted.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TeacherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTeacherById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTeacherByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Returns a paginated list of all active teachers with optional filters.</summary>
    /// <response code="200">Paginated list of teachers.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TeacherSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTeachers(
        [FromQuery] string? firstName,
        [FromQuery] string? lastName,
        [FromQuery] string? email,
        [FromQuery] Guid? subjectId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new GetAllTeachersQuery(firstName, lastName, email, subjectId, pageNumber, pageSize);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>Updates an existing teacher's profile fields.</summary>
    /// <response code="200">Returns the updated teacher profile.</response>
    /// <response code="400">Validation error — no updatable fields provided.</response>
    /// <response code="404">Teacher not found.</response>
    /// <response code="409">New email already belongs to another teacher.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TeacherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateTeacher(
        Guid id,
        [FromBody] UpdateTeacherCommand command,
        CancellationToken ct)
    {
        // Route id is authoritative — replace whatever TeacherId came in the body.
        var result = await _mediator.Send(command with { TeacherId = id }, ct);
        return Ok(result);
    }

    /// <summary>Soft-deletes a teacher by ID.</summary>
    /// <response code="204">Teacher deleted successfully.</response>
    /// <response code="404">Teacher not found.</response>
    /// <response code="422">Teacher has active course assignments that must be removed first.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteTeacher(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteTeacherCommand(id), ct);
        return NoContent();
    }

    // ── Subject Assignments ───────────────────────────────────────────────────

    /// <summary>Assigns a subject to a teacher.</summary>
    /// <response code="204">Subject assigned successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Teacher or subject not found.</response>
    /// <response code="409">Subject is already assigned to this teacher.</response>
    [HttpPost("{id:guid}/subjects")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignSubject(
        Guid id,
        [FromBody] AssignSubjectRequest request,
        CancellationToken ct)
    {
        await _mediator.Send(new AssignSubjectToTeacherCommand(id, request.SubjectId), ct);
        return NoContent();
    }

    /// <summary>Removes a subject assignment from a teacher.</summary>
    /// <response code="204">Subject removed successfully.</response>
    /// <response code="404">Teacher not found, or subject not assigned to this teacher.</response>
    [HttpDelete("{id:guid}/subjects/{subjectId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveSubject(Guid id, Guid subjectId, CancellationToken ct)
    {
        await _mediator.Send(new RemoveSubjectFromTeacherCommand(id, subjectId), ct);
        return NoContent();
    }

    /// <summary>Returns all subjects currently assigned to a teacher.</summary>
    /// <response code="200">List of assigned subjects.</response>
    /// <response code="404">Teacher not found.</response>
    [HttpGet("{id:guid}/subjects")]
    [ProducesResponseType(typeof(IReadOnlyList<SubjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubjectsByTeacher(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSubjectsByTeacherQuery(id), ct);
        return Ok(result);
    }

    // ── Availability ──────────────────────────────────────────────────────────

    /// <summary>Replaces a teacher's availability slots (pass empty list to clear all).</summary>
    /// <response code="204">Availability updated successfully.</response>
    /// <response code="400">Validation error — invalid slot (startTime >= endTime).</response>
    /// <response code="404">Teacher not found.</response>
    [HttpPut("{id:guid}/availability")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetAvailability(
        Guid id,
        [FromBody] SetTeacherAvailabilityCommand command,
        CancellationToken ct)
    {
        await _mediator.Send(command with { TeacherId = id }, ct);
        return NoContent();
    }

    /// <summary>Returns all availability slots for a teacher.</summary>
    /// <response code="200">List of availability slots.</response>
    /// <response code="404">Teacher not found.</response>
    [HttpGet("{id:guid}/availability")]
    [ProducesResponseType(typeof(IReadOnlyList<AvailabilitySlotDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAvailability(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTeacherAvailabilityQuery(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Returns all teachers whose availability overlaps the requested day and time range.
    /// NOTE: This route must be declared before <c>GET /{id}</c> to avoid Guid-parse conflicts.
    /// </summary>
    /// <response code="200">List of available teachers.</response>
    /// <response code="400">Validation error — startTime >= endTime.</response>
    [HttpGet("available")]
    [ProducesResponseType(typeof(IReadOnlyList<TeacherSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAvailableTeachers(
        [FromQuery] DayOfWeek dayOfWeek,
        [FromQuery] TimeOnly startTime,
        [FromQuery] TimeOnly endTime,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAvailableTeachersQuery(dayOfWeek, startTime, endTime), ct);
        return Ok(result);
    }

    // ── Course / Schedule ─────────────────────────────────────────────────────

    /// <summary>Assigns a teacher to a course with a specific time slot.</summary>
    /// <response code="204">Teacher assigned to course successfully.</response>
    /// <response code="400">Validation error — missing fields or startTime >= endTime.</response>
    /// <response code="404">Teacher or course not found.</response>
    /// <response code="422">Overlapping schedule entry already exists for this teacher.</response>
    [HttpPost("{id:guid}/courses")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AssignToCourse(
        Guid id,
        [FromBody] AssignToCourseRequest request,
        CancellationToken ct)
    {
        await _mediator.Send(
            new AssignTeacherToCourseCommand(id, request.CourseId, request.DayOfWeek, request.StartTime, request.EndTime),
            ct);
        return NoContent();
    }

    /// <summary>Removes a teacher's course assignment.</summary>
    /// <response code="204">Teacher removed from course successfully.</response>
    /// <response code="404">Teacher not found, or course not in teacher's schedule.</response>
    [HttpDelete("{id:guid}/courses/{courseId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromCourse(Guid id, Guid courseId, CancellationToken ct)
    {
        await _mediator.Send(new RemoveTeacherFromCourseCommand(id, courseId), ct);
        return NoContent();
    }

    /// <summary>Returns a teacher's full schedule, optionally filtered by day.</summary>
    /// <response code="200">List of schedule entries.</response>
    /// <response code="404">Teacher not found.</response>
    [HttpGet("{id:guid}/schedule")]
    [ProducesResponseType(typeof(IReadOnlyList<ScheduleEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSchedule(
        Guid id,
        [FromQuery] DayOfWeek? dayOfWeek,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTeacherScheduleQuery(id, dayOfWeek), ct);
        return Ok(result);
    }
}

/*
   HTTP Request -> Controller -> Create Command/Query -> _mediator.Send() -> Application Handler -> 
   Domain -> Repository -> MongoDB -> Result -> Controller -> HTTP Response


   Controller-> Application -> Domain -> Infrastructure
*/
