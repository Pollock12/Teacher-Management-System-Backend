namespace TMS.Application.Teachers.DTOs;

public record TeacherSummaryDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email
);

/*
  TeacherDTO = full teacher information
  TeacherSummaryDTO = short/basic teacher information.

*/
