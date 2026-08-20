using MediatR;
using TMS.Application.Common;
using TMS.Application.Teachers.DTOs;
using TMS.Domain.Repositories;

namespace TMS.Application.Teachers.Queries.GetAllTeachers;

/// <summary>
/// Handles <see cref="GetAllTeachersQuery"/>.
/// Delegates paging and filtering to the repository, then maps each result to a
/// lightweight <see cref="TeacherSummaryDto"/> and wraps everything in a
/// <see cref="PagedResult{T}"/>.
/// Satisfies Requirements 4.3, 4.4, 4.5, 4.6.
/// </summary>
public sealed class GetAllTeachersQueryHandler
    : IRequestHandler<GetAllTeachersQuery, PagedResult<TeacherSummaryDto>>
{
    private readonly ITeacherRepository _teacherRepository;

    public GetAllTeachersQueryHandler(ITeacherRepository teacherRepository)
    {
        _teacherRepository = teacherRepository;
    }

    public async Task<PagedResult<TeacherSummaryDto>> Handle(
        GetAllTeachersQuery query,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _teacherRepository.GetPagedAsync(
            firstName: query.FirstName,
            lastName: query.LastName,
            email: query.Email,
            subjectId: query.SubjectId,
            pageNumber: query.PageNumber,
            pageSize: query.PageSize,
            ct: cancellationToken);

        var summaries = items
            .Select(t => t.ToSummaryDto())
            .ToList();

        return new PagedResult<TeacherSummaryDto>(
            Items: summaries,
            TotalCount: totalCount,
            PageNumber: query.PageNumber,
            PageSize: query.PageSize);
    }
}
