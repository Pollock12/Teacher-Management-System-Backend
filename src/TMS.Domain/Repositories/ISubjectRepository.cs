using TMS.Domain.Aggregates.Subjects;

namespace TMS.Domain.Repositories;

public interface ISubjectRepository
{
    //Find a Subject using its ID.
    Task<Subject?> GetByIdAsync(Guid id, CancellationToken ct = default);

    //FInd a Subject by its name
    Task<Subject?> GetByNameAsync(string name, CancellationToken ct = default);

    //Get all Subjects that are currently active.
    Task<IReadOnlyList<Subject>> GetAllActiveAsync(CancellationToken ct = default);

    //Add a new Subject to the database
    Task AddAsync(Subject subject, CancellationToken ct = default);

    //Update a Subject to the database
    Task UpdateAsync(Subject subject, CancellationToken ct = default);

    //Check whether the Subject is currently assigned to atleast one Teacher.
    Task<bool> IsAssignedToAnyTeacherAsync(Guid subjectId, CancellationToken ct = default);
}
