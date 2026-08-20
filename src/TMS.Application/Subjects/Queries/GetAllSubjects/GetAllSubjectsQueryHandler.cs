using MediatR;
using TMS.Application.Subjects.DTOs;
using TMS.Domain.Repositories;

namespace TMS.Application.Subjects.Queries.GetAllSubjects;

/// <summary>
/// Handles <see cref="GetAllSubjectsQuery"/>.
/// Delegates to <see cref="ISubjectRepository.GetAllActiveAsync"/> and maps each
/// result to a <see cref="SubjectDto"/>.
/// Satisfies Requirement 5.4.
/// </summary>
public sealed class GetAllSubjectsQueryHandler
    : IRequestHandler<GetAllSubjectsQuery, IReadOnlyList<SubjectDto>>
{
    private readonly ISubjectRepository _subjectRepository;

    public GetAllSubjectsQueryHandler(ISubjectRepository subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    public async Task<IReadOnlyList<SubjectDto>> Handle(
        GetAllSubjectsQuery query,
        CancellationToken cancellationToken)
    {
        var subjects = await _subjectRepository.GetAllActiveAsync(cancellationToken);

        return subjects
            .Select(s => new SubjectDto(s.Id, s.Name, s.Description, s.CreatedAt, s.UpdatedAt))
            .ToList();
    }
}
