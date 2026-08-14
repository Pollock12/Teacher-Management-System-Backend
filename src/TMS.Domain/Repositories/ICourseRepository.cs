using TMS.Domain.Aggregates.Courses;

namespace TMS.Domain.Repositories;

public interface ICourseRepository
{
    //Find a Course using its Id.
    Task<Course?> GetByIdAsync(Guid id, CancellationToken ct = default);

    //Add a new Course to the database
    Task AddAsync(Course course, CancellationToken ct = default);
}
