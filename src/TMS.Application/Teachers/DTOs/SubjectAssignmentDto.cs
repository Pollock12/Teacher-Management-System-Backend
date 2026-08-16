namespace TMS.Application.Teachers.DTOs;

public record SubjectAssignmentDto(Guid SubjectId, DateTime AssignedAt);

/*
  SubjectAssignment = Domain object used inside your business logic.
  SubjectAssignmentDTO = Data represenation sent between Application/API/Frontend.
*/