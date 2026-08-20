using MediatR;
using TMS.Application.Subjects.DTOs;
using TMS.Domain.Exceptions;
using TMS.Domain.Repositories;

namespace TMS.Application.Teachers.Queries.GetSubjectsByTeacher;

/// <summary>
/// Handles <see cref="GetSubjectsByTeacherQuery"/>.
/// Loads the teacher, throws <see cref="NotFoundException"/> when not found, then resolves
/// each <see cref="TMS.Domain.ValueObjects.SubjectAssignment"/> to a full <see cref="SubjectDto"/>
/// by loading the subject from <see cref="ISubjectRepository"/>.
/// Satisfies Requirement 6.6.
/// </summary>
public sealed class GetSubjectsByTeacherQueryHandler
    : IRequestHandler<GetSubjectsByTeacherQuery, IReadOnlyList<SubjectDto>>
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly ISubjectRepository _subjectRepository;

    public GetSubjectsByTeacherQueryHandler(
        ITeacherRepository teacherRepository,
        ISubjectRepository subjectRepository)
    {
        _teacherRepository = teacherRepository;
        _subjectRepository = subjectRepository;
    }

    public async Task<IReadOnlyList<SubjectDto>> Handle(
        GetSubjectsByTeacherQuery query,
        CancellationToken cancellationToken)
    {
        var teacher = await _teacherRepository.GetByIdAsync(query.TeacherId, cancellationToken);

        if (teacher is null)
            throw new NotFoundException($"Teacher with ID '{query.TeacherId}' was not found.");

        // Load each subject concurrently to avoid sequential round-trips.
        var subjectTasks = teacher.SubjectAssignments
            .Select(a => _subjectRepository.GetByIdAsync(a.SubjectId, cancellationToken));

        var subjects = await Task.WhenAll(subjectTasks);

        return subjects
            .Where(s => s is not null)   // guard against orphaned assignments
            .Select(s => new SubjectDto(s!.Id, s.Name, s.Description, s.CreatedAt, s.UpdatedAt))
            .ToList();
    }
}

/*
   This handler first finds the teacher, reads the subject IDs assigned to that teacher, 
   loads those subjects from MongoDB, converts them into SubjectDtos, 
   and returns the list to the API layer.
*/
