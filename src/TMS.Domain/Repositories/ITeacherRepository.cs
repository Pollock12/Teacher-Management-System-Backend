using TMS.Domain.Aggregates.Teachers;

namespace TMS.Domain.Repositories;

public interface ITeacherRepository
{
    /*1.Find a teacher using their ID
    2.The ? means the Teacher maybe null
    3.Why Task? -> means the operation is asynchronous
    4.CancellationToken -> this allows the operation to be cancelled.
    For example, suppose a user request a page.
    But the user closes the page before the query finishes.
    The opearion can be cancelled using the CancellationToken.*/
    Task<Teacher?> GetByIdAsync(Guid id, CancellationToken ct = default);

    // This is useful when you need to check whether an email already exists.
    Task<Teacher?> GetByEmailAsync(string email, CancellationToken ct = default);

    //It is used when you want to retreive a page of teachers, possibly with filters.
    Task<(IReadOnlyList<Teacher> Items, int TotalCount)> GetPagedAsync(
        string? firstName, string? lastName, string? email, Guid? subjectId,
        int pageNumber, int pageSize, CancellationToken ct = default);

    //Find teachers who are available during a particular time slot
    Task<IReadOnlyList<Teacher>> GetAvailableAsync(
        DayOfWeek day, TimeOnly startTime, TimeOnly endTime, CancellationToken ct = default);

    //Add a new Teacher to the database
    Task AddAsync(Teacher teacher, CancellationToken ct = default);

    //Update a Teacher to the database
    Task UpdateAsync(Teacher teacher, CancellationToken ct = default);
}
