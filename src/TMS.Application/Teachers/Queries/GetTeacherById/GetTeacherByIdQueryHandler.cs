using MediatR;
using TMS.Application.Teachers.DTOs;
using TMS.Domain.Exceptions;
using TMS.Domain.Repositories;

namespace TMS.Application.Teachers.Queries.GetTeacherById;

/// <summary>
/// Handles <see cref="GetTeacherByIdQuery"/>.
/// Loads the teacher from the repository (which already filters out soft-deleted records),
/// throws <see cref="NotFoundException"/> when the result is null, and returns a mapped DTO.
/// Satisfies Requirements 4.1, 4.2.
/// </summary>
public sealed class GetTeacherByIdQueryHandler : IRequestHandler<GetTeacherByIdQuery, TeacherDto>
{
    private readonly ITeacherRepository _teacherRepository;

    public GetTeacherByIdQueryHandler(ITeacherRepository teacherRepository)
    {
        _teacherRepository = teacherRepository;
    }

    public async Task<TeacherDto> Handle(
        GetTeacherByIdQuery query,
        CancellationToken cancellationToken)
    {
        // Repository filters IsDeleted == false, so null means not found or deleted.
        var teacher = await _teacherRepository.GetByIdAsync(query.TeacherId, cancellationToken);

        if (teacher is null)
            throw new NotFoundException($"Teacher with ID '{query.TeacherId}' was not found.");

        return teacher.ToDto();
    }
}
