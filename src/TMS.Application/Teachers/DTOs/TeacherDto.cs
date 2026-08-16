namespace TMS.Application.Teachers.DTOs;

public record TeacherDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string? Address,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<SubjectAssignmentDto> SubjectAssignments,
    IReadOnlyList<AvailabilitySlotDto> AvailabilitySlots,
    IReadOnlyList<ScheduleEntryDto> ScheduleEntries
);

/*
   This TeacherDTO is a DTO(Data Transfer Object).
   Its job is to carry teacher data from one layer to another, especially from the Application layer -> API layer.
   It is basically a data container.
   It does not contain business logic like AssignSubject(), SoftDelete(),AssignToCourse()

   *** Why use DTO instead of returning Teacher directly?
   => Your domain object is Teacher -> business rules,methods,domain events,private collections,properties.
   You usually don't want to expose all of that to the API/Client.
   So the DTO acts like a safe data representation.

   *** Why is it a record?
   => A record is convenient for DTOs because DTOs are mainly data, not behavior.

   *** The complete flow
   MongoDB -> TeacherRepository -> Teacher -> TeacherDTO -> API -> Angular

   1. MongoDB stores the data.
   2. TeacherRepository gets the data.(the repository goes to MongoDB and asks : Give me the teacher with this ID)
   3. MongoDB data becomes a Teacher. MongoDB driver uses your mapping configuration and creates a C# object.(Teacher teacher).Teacher is not just data. It also contains your business rules.
   4. Why convert Teacher to TeacherDTO? You don't normally want to send your domain object directly to Angular.Instead you create TeacherDTO.
   5. The API receives the TeacherDTO. The API converts it to JSON.
   6. Angular receives the JSON. (GET/api/teacher/123).
*/
