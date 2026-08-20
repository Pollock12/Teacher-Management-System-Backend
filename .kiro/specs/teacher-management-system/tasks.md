# Implementation Plan: Teacher Management System (TMS)

## Overview

Implement a .NET 8 / C# backend API following DDD and CQRS using MediatR, FluentValidation, and MongoDB. The solution consists of four projects: `TMS.Domain`, `TMS.Application`, `TMS.Infrastructure`, and `TMS.API`, plus a `TMS.Tests` project. Tasks are ordered so that lower-level layers are implemented before the layers that depend on them.

---

## Tasks

- [x] 1. Scaffold solution and project structure
  - [x] 1.1 Create solution file and four source projects with correct project references
    - Create `TeacherManagementSystem.sln`
    - Create `src/TMS.Domain`, `src/TMS.Application`, `src/TMS.Infrastructure`, `src/TMS.API` as class-library / web-api projects targeting .NET 8
    - Add project references: Application → Domain; Infrastructure → Domain; API → Application + Infrastructure
    - Add NuGet packages per project: `MongoDB.Driver` (Infrastructure), `MediatR` + `FluentValidation.AspNetCore` (Application), `Swashbuckle.AspNetCore` (API)
    - _Requirements: all — foundational step_

  - [x] 1.2 Create test project with required NuGet packages
    - Create `tests/TMS.Tests` (xUnit project) referencing all four source projects
    - Add `xUnit`, `FluentAssertions`, `FsCheck.Xunit`, `Testcontainers.MongoDb`, `Microsoft.NET.Test.Sdk`
    - _Requirements: all — foundational step_

---

- [x] 2. Implement Domain layer — base classes and value objects
  - [x] 2.1 Implement `Entity` base class and `IDomainEvent` interface
    - Create `TMS.Domain/Common/Entity.cs` with `Id`, `CreatedAt`, `UpdatedAt`, private `_domainEvents` list, `AddDomainEvent`, `ClearDomainEvents`, and `DomainEvents` read-only property
    - Create `TMS.Domain/Common/IDomainEvent.cs` with `EventId`, `OccurredAt`, `EventType`
    - _Requirements: 10.1–10.6_

  - [x] 2.2 Implement value objects: `AvailabilitySlot`, `ScheduleEntry`, `SubjectAssignment`
    - Create `TMS.Domain/ValueObjects/AvailabilitySlot.cs` — sealed, immutable; constructor enforces `StartTime < EndTime`; implement `Overlaps(DayOfWeek, TimeOnly, TimeOnly)` method
    - Create `TMS.Domain/ValueObjects/ScheduleEntry.cs` — sealed, immutable; constructor enforces `StartTime < EndTime`; implement `ConflictsWith(DayOfWeek, TimeOnly, TimeOnly)` method
    - Create `TMS.Domain/ValueObjects/SubjectAssignment.cs` — sealed, immutable; constructor rejects empty `SubjectId`; sets `AssignedAt = DateTime.UtcNow`
    - _Requirements: 7.2, 7.3, 8.2, 8.3, 6.4_

  - [x] 2.3 Write property tests for value objects
    - **Property 4: Availability Slot Validity** — for any `AvailabilitySlot`, `StartTime < EndTime` always holds; construction with `StartTime >= EndTime` always throws
    - **Validates: Requirements 7.3**
    - **Property 3: Schedule Conflict Prevention** — for any two non-overlapping time ranges on the same day, `ConflictsWith` returns false; for any overlapping pair, it returns true
    - **Validates: Requirements 8.3**
    - Use `FsCheck.Xunit` `[Property]` attribute

  - [x] 2.4 Implement custom domain exceptions
    - Create `TMS.Domain/Exceptions/NotFoundException.cs`
    - Create `TMS.Domain/Exceptions/ConflictException.cs`
    - Create `TMS.Domain/Exceptions/DomainRuleException.cs`
    - _Requirements: 9.2, 9.3, 9.4_

---

- [ ] 3. Implement Domain layer — aggregates
  - [x] 3.1 Implement `Teacher` aggregate root
    - Create `TMS.Domain/Aggregates/Teachers/Teacher.cs` — sealed; private constructor; `Create()` factory sets all fields and raises `TeacherCreated` event
    - Implement `Update()`, `SoftDelete()`, `AssignSubject()`, `RemoveSubject()`, `SetAvailability()`, `AssignToCourse()`, `RemoveFromCourse()` with their pre/postconditions as described in design §2.4
    - _Requirements: 1.1, 1.5, 2.1, 3.1, 3.4, 6.1, 6.5, 7.1, 8.1, 8.6, 10.1–10.5_

  - [x] 3.2 Implement `Subject` aggregate root and `Course` aggregate root
    - Create `TMS.Domain/Aggregates/Subjects/Subject.cs` — sealed; `Create()` factory; `SoftDelete()` method
    - Create `TMS.Domain/Aggregates/Courses/Course.cs` — sealed; `Create()` factory
    - _Requirements: 5.1, 5.5_

  - [x] 3.3 Implement domain events
    - Create `TMS.Domain/Aggregates/Teachers/Events/DomainEventBase.cs` — abstract record with `EventId` and `OccurredAt` defaults
    - Create `TeacherCreated.cs`, `TeacherUpdated.cs`, `TeacherDeleted.cs`, `SubjectAssignedToTeacher.cs`, `TeacherAssignedToCourse.cs` event records in the same folder
    - _Requirements: 10.1–10.5_

  - [ ]* 3.4 Write unit tests for `Teacher` aggregate domain logic
    - Test `SoftDelete()` throws `DomainRuleException` when schedule entries exist (Property 7)
    - Test `AssignToCourse()` throws `DomainRuleException` on overlapping time slot (Property 3)
    - Test `AssignSubject()` throws `ConflictException` on duplicate subject (Property 5)
    - Test `Create()` raises exactly one `TeacherCreated` event (Property 6)
    - Test `SoftDelete()` sets `IsDeleted = true` and raises `TeacherDeleted` event
    - Test `AssignToCourse()` raises `TeacherAssignedToCourse` event on success
    - _Requirements: 3.4, 6.4, 8.3, 10.1, 10.3, 10.5_

  - [ ]* 3.5 Write property tests for `Teacher` aggregate invariants
    - **Property 5: Subject Assignment Uniqueness** — assigning the same `SubjectId` twice to the same teacher always raises `ConflictException`
    - **Validates: Requirements 6.4**
    - **Property 7: Delete Safety** — a teacher with any `ScheduleEntry` always throws on `SoftDelete()`
    - **Validates: Requirements 3.4**
    - **Property 10: Event Ordering** — `OccurredAt` timestamps across multiple events raised in a single command flow are monotonically non-decreasing
    - **Validates: Requirements 10.6**

---

- [x] 4. Implement Domain layer — repository interfaces
  - [-] 4.1 Define repository interfaces
    - Create `TMS.Domain/Repositories/ITeacherRepository.cs` with `GetByIdAsync`, `GetByEmailAsync`, `GetPagedAsync`, `GetAvailableAsync`, `AddAsync`, `UpdateAsync`
    - Create `TMS.Domain/Repositories/ISubjectRepository.cs` with `GetByIdAsync`, `GetByNameAsync`, `GetAllActiveAsync`, `AddAsync`, `UpdateAsync`, `IsAssignedToAnyTeacherAsync`
    - Create `TMS.Domain/Repositories/ICourseRepository.cs` with `GetByIdAsync`, `AddAsync`
    - Create `TMS.Domain/Repositories/IDomainEventRepository.cs` with `PersistAsync`
    - _Requirements: 1.1, 4.3, 5.4, 7.6_

---

- [x] 5. Implement Infrastructure layer
  - [x] 5.1 Implement `MongoDbSettings` and `MongoDbContext`
    - Create `TMS.Infrastructure/Persistence/MongoDbContext.cs` with strongly-typed collection properties: `Teachers`, `Subjects`, `Courses`, `DomainEvents`
    - Create `MongoDbSettings` class with `ConnectionString` and `DatabaseName` properties
    - _Requirements: 1.6_

  - [x] 5.2 Implement BSON mapping configuration
    - Create `TMS.Infrastructure/Persistence/Mappings/BsonMappingConfiguration.cs`
    - Register `GuidSerializer` (Standard representation), `TimeOnlySerializer`, `DateOnlySerializer`
    - Register class maps for `Teacher`, `Subject`, `Course` — unmapping `DomainEvents` from the document
    - _Requirements: 1.1, 1.6_

  - [x] 5.3 Implement `TeacherRepository`
    - Create `TMS.Infrastructure/Persistence/Repositories/TeacherRepository.cs` implementing `ITeacherRepository`
    - `GetByIdAsync`: filter `IsDeleted == false && Id == id`
    - `GetByEmailAsync`: filter `IsDeleted == false && Email == email`
    - `GetPagedAsync`: build compound filter with regex for name/email fields, `ElemMatch` for `subjectId`; return `(items, totalCount)`
    - `GetAvailableAsync`: `ElemMatch` on `AvailabilitySlots` checking `DayOfWeek`, `StartTime < endTime`, `EndTime > startTime`
    - `AddAsync`, `UpdateAsync` (replace-one)
    - _Requirements: 1.4, 2.3, 3.3, 4.3, 4.4, 4.5, 7.6_

  - [x] 5.4 Implement `SubjectRepository` and `CourseRepository`
    - Create `SubjectRepository.cs` implementing `ISubjectRepository`; implement `IsAssignedToAnyTeacherAsync` via filter on `teachers` collection's `subjectAssignments` array
    - Create `CourseRepository.cs` implementing `ICourseRepository`
    - _Requirements: 5.3, 5.6, 8.4, 8.5_

  - [x] 5.5 Implement `DomainEventRepository` and `DomainEventDocument`
    - Create `DomainEventRepository.cs` implementing `IDomainEventRepository`; serialize each `IDomainEvent` to JSON payload and bulk-insert into `domain_events` collection
    - Define `DomainEventDocument` class with `Id`, `EventType`, `OccurredAt`, `Payload` (JSON string)
    - _Requirements: 10.6_

  - [x] 5.6 Implement `DependencyInjection` extension method
    - Create `TMS.Infrastructure/DependencyInjection.cs` with `AddInfrastructure(IServiceCollection, IConfiguration)` extension
    - Bind `MongoDbSettings` from `"MongoDB"` configuration section
    - Register `MongoDbContext` as singleton; register all four repositories as scoped
    - _Requirements: 1.6_

  - [ ]* 5.7 Write integration tests for repository implementations
    - Use `Testcontainers.MongoDb` to spin up a real MongoDB container in tests
    - Test `TeacherRepository.AddAsync` then `GetByIdAsync` round-trips correctly
    - Test `GetPagedAsync` returns correct `TotalCount` for various filter combinations
    - Test `DomainEventRepository.PersistAsync` inserts events into `domain_events` collection
    - Test `SubjectRepository.IsAssignedToAnyTeacherAsync` returns `true` when subject is assigned
    - _Requirements: 1.6, 4.5, 10.6_

---

- [x] 6. Implement Application layer — shared infrastructure
  - [-] 6.1 Implement DTOs and `PagedResult` wrapper
    - Create `TMS.Application/Teachers/DTOs/TeacherDto.cs`, `TeacherSummaryDto.cs`, `AvailabilitySlotDto.cs`, `ScheduleEntryDto.cs`, `SubjectAssignmentDto.cs`
    - Create `TMS.Application/Common/PagedResult.cs` generic record with `Items`, `TotalCount`, `PageNumber`, `PageSize`
    - Create `TMS.Application/Subjects/DTOs/SubjectDto.cs`
    - Create `TMS.Application/Courses/DTOs/CourseDto.cs`
    - _Requirements: 4.3, 4.5, 4.6_

  - [x] 6.2 Implement `ValidationBehavior` MediatR pipeline behavior
    - Create `TMS.Application/Common/Behaviors/ValidationBehavior.cs`
    - Inject `IEnumerable<IValidator<TRequest>>`; run all validators; collect failures; throw `ValidationException` if any failures exist; otherwise call `next()`
    - _Requirements: 9.1_

  - [x] 6.3 Add mapping extension methods for aggregates to DTOs
    - Create `TMS.Application/Teachers/DTOs/TeacherMappingExtensions.cs` with `ToDto()` extension on `Teacher`
    - _Requirements: 1.5, 2.5, 4.1_

---

- [x] 7. Implement Application layer — Teacher commands
  - [x] 7.1 Implement `CreateTeacherCommand`, validator, and handler
    - Create command record `CreateTeacherCommand(FirstName, LastName, Email, PhoneNumber?, DateOfBirth?, Address?) : IRequest<TeacherDto>`
    - Create `CreateTeacherCommandValidator` — `NotEmpty` + `MaximumLength(100)` for names; `NotEmpty` + `EmailAddress` + `MaximumLength(255)` for email
    - Implement `CreateTeacherCommandHandler`: check email uniqueness → `ConflictException`; call `Teacher.Create()`; persist domain events; call `AddAsync`; return mapped `TeacherDto`
    - _Requirements: 1.1–1.6, 9.1, 9.3, 10.1_

  - [ ]* 7.2 Write property test for email uniqueness invariant
    - **Property 1: Email Uniqueness** — attempting to create two teachers with the same email always raises `ConflictException`, regardless of the other field values
    - **Validates: Requirements 1.4**

  - [x] 7.3 Implement `UpdateTeacherCommand`, validator, and handler
    - Create command record and `UpdateTeacherCommandValidator` — at-least-one-field rule + optional email format check
    - Implement handler: load by ID → `NotFoundException`; check email uniqueness if changed → `ConflictException`; call `teacher.Update()`; persist events; call `UpdateAsync`; return `TeacherDto`
    - _Requirements: 2.1–2.5, 9.2, 9.3, 10.2_

  - [x] 7.4 Implement `DeleteTeacherCommand` and handler
    - Create command record `DeleteTeacherCommand(TeacherId) : IRequest`
    - Implement handler: load by ID → `NotFoundException`; call `teacher.SoftDelete()` (throws `DomainRuleException` if active assignments); persist events; call `UpdateAsync`
    - _Requirements: 3.1–3.4, 9.4, 10.3_

  - [x] 7.5 Implement `AssignSubjectToTeacherCommand`, validator, and handler
    - Create command and `AssignSubjectToTeacherCommandValidator` (`NotEmpty` for both IDs)
    - Implement handler: load teacher → `NotFoundException`; load subject → `NotFoundException`; call `teacher.AssignSubject()` → `ConflictException` on duplicate; persist events; `UpdateAsync`
    - _Requirements: 6.1–6.4, 9.2, 9.3, 10.4_

  - [x] 7.6 Implement `RemoveSubjectFromTeacherCommand` and handler
    - Create command record
    - Implement handler: load teacher → `NotFoundException`; call `teacher.RemoveSubject()` → `NotFoundException` if not assigned; `UpdateAsync`
    - _Requirements: 6.5_

  - [x] 7.7 Implement `SetTeacherAvailabilityCommand`, validator, and handler
    - Create command with `Slots : IReadOnlyList<AvailabilitySlotInput>`; create `AvailabilitySlotInput` record
    - Create `SetTeacherAvailabilityCommandValidator` — `TeacherId.NotEmpty`; `RuleForEach` slots → `StartTime < EndTime`
    - Implement handler: load teacher → `NotFoundException`; map inputs to `AvailabilitySlot` value objects; `teacher.SetAvailability(slots)`; `UpdateAsync`
    - _Requirements: 7.1–7.4_

  - [x] 7.8 Implement `AssignTeacherToCourseCommand`, validator, and handler
    - Create command with `TeacherId`, `CourseId`, `DayOfWeek`, `StartTime`, `EndTime`
    - Create validator: `NotEmpty` for IDs; `StartTime < EndTime`
    - Implement handler: load teacher → `NotFoundException`; load course → `NotFoundException`; `teacher.AssignToCourse()` → `DomainRuleException` on conflict; persist events; `UpdateAsync`
    - _Requirements: 8.1–8.5, 9.4, 10.5_

  - [ ]* 7.9 Write property test for schedule conflict prevention
    - **Property 3: Schedule Conflict Prevention** — for any teacher with an existing schedule entry, assigning a new overlapping slot on the same day always raises `DomainRuleException`; non-overlapping slots always succeed
    - **Validates: Requirements 8.3**

  - [x] 7.10 Implement `RemoveTeacherFromCourseCommand` and handler
    - Create command record `RemoveTeacherFromCourseCommand(TeacherId, CourseId) : IRequest`
    - Implement handler: load teacher → `NotFoundException`; `teacher.RemoveFromCourse()` → `NotFoundException` if entry not found; `UpdateAsync`
    - _Requirements: 8.6_

---

- [x] 8. Implement Application layer — Subject commands and query
  - [x] 8.1 Implement `CreateSubjectCommand`, validator, and handler
    - Create command record `CreateSubjectCommand(Name, Description?) : IRequest<SubjectDto>`
    - Create `CreateSubjectCommandValidator` — `NotEmpty` + `MaximumLength(200)` for `Name`
    - Implement handler: check name uniqueness → `ConflictException`; `Subject.Create()`; `AddAsync`; return `SubjectDto`
    - _Requirements: 5.1–5.3_

  - [x] 8.2 Implement `DeleteSubjectCommand` and handler
    - Create command record `DeleteSubjectCommand(SubjectId) : IRequest`
    - Implement handler: load subject → `NotFoundException`; check `IsAssignedToAnyTeacherAsync` → `DomainRuleException` if true; `subject.SoftDelete()`; `UpdateAsync`
    - _Requirements: 5.5, 5.6_

  - [ ]* 8.3 Write property test for subject delete safety
    - **Property 8: Subject Delete Safety** — for any subject that is assigned to at least one teacher, `DeleteSubjectCommandHandler` always raises `DomainRuleException`
    - **Validates: Requirements 5.6**

---

- [ ] 9. Implement Application layer — all queries
  - [x] 9.1 Implement `GetTeacherByIdQuery` and handler
    - Create query record; implement handler: load by ID → `NotFoundException` if not found or `IsDeleted`; return `TeacherDto`
    - _Requirements: 4.1, 4.2_

  - [ ]* 9.2 Write property test for soft-delete visibility
    - **Property 2: Soft-Delete Visibility** — after `SoftDelete()`, `GetTeacherByIdQuery` always returns a `NotFoundException` for that teacher ID
    - **Validates: Requirements 3.3**

  - [x] 9.3 Implement `GetAllTeachersQuery` and handler
    - Create query record with filter params and pagination defaults (`PageSize = 20`); implement handler calling `GetPagedAsync`; map to `PagedResult<TeacherSummaryDto>`
    - _Requirements: 4.3–4.6_

  - [ ]* 9.4 Write property test for pagination correctness
    - **Property 9: Pagination Correctness** — `PagedResult.TotalCount` equals the total number of matching documents regardless of `PageSize`; varying `PageNumber` and `PageSize` does not change `TotalCount`
    - **Validates: Requirements 4.5**

  - [x] 9.5 Implement `GetTeacherAvailabilityQuery` and handler
    - Create query record; implement handler: load teacher → `NotFoundException`; map `AvailabilitySlots` to `IReadOnlyList<AvailabilitySlotDto>`
    - _Requirements: 7.5_

  - [x] 9.6 Implement `GetAvailableTeachersQuery` and handler
    - Create query record with `DayOfWeek`, `StartTime`, `EndTime`; implement handler calling `repository.GetAvailableAsync`; map to `IReadOnlyList<TeacherSummaryDto>`
    - _Requirements: 7.6_

  - [x] 9.7 Implement `GetSubjectsByTeacherQuery` and handler
    - Create query record; implement handler: load teacher → `NotFoundException`; load each subject by `SubjectId`; map to `IReadOnlyList<SubjectDto>`
    - _Requirements: 6.6_

  - [x] 9.8 Implement `GetTeacherScheduleQuery` and handler
    - Create query record with optional `DayOfWeek?`; implement handler: load teacher → `NotFoundException`; filter `ScheduleEntries` by day if provided; map to `IReadOnlyList<ScheduleEntryDto>`
    - _Requirements: 8.7, 8.8_

  - [ ] 9.9 Implement `GetAllSubjectsQuery` and handler
    - Create query record; implement handler: call `subjectRepository.GetAllActiveAsync()`; map to `IReadOnlyList<SubjectDto>`
    - _Requirements: 5.4_

---

- [ ] 10. Checkpoint — domain and application layer complete
  - Ensure all domain, application command, and query unit tests pass. Ask the user if any design clarifications are needed before proceeding to API layer.

---

- [ ] 11. Implement Infrastructure layer — MongoDB indexes
  - [ ] 11.1 Create startup index-creation logic
    - Add an `EnsureIndexesAsync()` method on `MongoDbContext` (or a separate `MongoDbIndexInitializer`) that creates: unique sparse index on `teachers.email`; index on `teachers.isDeleted`; index on `teachers.subjectAssignments.subjectId`; indexes on `domain_events.occurredAt` and `domain_events.eventType`
    - Call this method from `Program.cs` at startup
    - _Requirements: 1.6 (performance — ensures query efficiency)_

---

- [ ] 12. Implement API layer
  - [ ] 12.1 Implement `ErrorResponse` model and `GlobalExceptionMiddleware`
    - Create `TMS.API/Models/ErrorResponse.cs` with `CorrelationId`, `Status`, `Error`, `Message`, `Details` properties
    - Create `TMS.API/Middleware/GlobalExceptionMiddleware.cs` mapping `ValidationException → 400`, `NotFoundException → 404`, `ConflictException → 409`, `DomainRuleException → 422`, catch-all `Exception → 500` with structured `ErrorResponse` JSON; log unhandled exceptions with `correlationId`
    - _Requirements: 9.1–9.6_

  - [ ] 12.2 Implement `TeachersController`
    - Create `TMS.API/Controllers/TeachersController.cs` with all endpoints as specified in design §2.18:
      - `POST /api/teachers` → `CreateTeacherCommand` → `201 Created`
      - `GET /api/teachers/{id}` → `GetTeacherByIdQuery` → `200 OK`
      - `GET /api/teachers` → `GetAllTeachersQuery` → `200 OK`
      - `PUT /api/teachers/{id}` → `UpdateTeacherCommand` → `200 OK`
      - `DELETE /api/teachers/{id}` → `DeleteTeacherCommand` → `204 No Content`
      - `POST /api/teachers/{id}/subjects` → `AssignSubjectToTeacherCommand` → `204 No Content`
      - `DELETE /api/teachers/{id}/subjects/{subjectId}` → `RemoveSubjectFromTeacherCommand` → `204 No Content`
      - `GET /api/teachers/{id}/subjects` → `GetSubjectsByTeacherQuery` → `200 OK`
      - `PUT /api/teachers/{id}/availability` → `SetTeacherAvailabilityCommand` → `204 No Content`
      - `GET /api/teachers/{id}/availability` → `GetTeacherAvailabilityQuery` → `200 OK`
      - `GET /api/teachers/available` → `GetAvailableTeachersQuery` → `200 OK`
      - `POST /api/teachers/{id}/courses` → `AssignTeacherToCourseCommand` → `204 No Content`
      - `DELETE /api/teachers/{id}/courses/{courseId}` → `RemoveTeacherFromCourseCommand` → `204 No Content`
      - `GET /api/teachers/{id}/schedule` → `GetTeacherScheduleQuery` → `200 OK`
    - Define `AssignSubjectRequest` record for the request body of the assign-subject endpoint
    - _Requirements: 1.1–10.6 (all surface via this controller)_

  - [ ] 12.3 Implement `SubjectsController`
    - Create `TMS.API/Controllers/SubjectsController.cs`:
      - `POST /api/subjects` → `CreateSubjectCommand` → `201 Created`
      - `GET /api/subjects` → `GetAllSubjectsQuery` → `200 OK`
      - `DELETE /api/subjects/{id}` → `DeleteSubjectCommand` → `204 No Content`
    - _Requirements: 5.1–5.6_

  - [ ] 12.4 Implement `CoursesController`
    - Create `TMS.API/Controllers/CoursesController.cs`:
      - `POST /api/courses` → create and dispatch a `CreateCourseCommand` (create command + handler if not yet implemented)
      - `GET /api/courses/{id}` → load via `ICourseRepository` and return `CourseDto`
    - _Requirements: 8.4, 8.5 (course must exist before teacher assignment)_

  - [ ] 12.5 Wire up `Program.cs` and `appsettings.json`
    - Configure `Program.cs`:
      - Call `BsonMappingConfiguration.Configure()` before any services
      - Register `AddControllers`, `AddEndpointsApiExplorer`, `AddSwaggerGen`
      - Register MediatR with assembly scanning on `TMS.Application` + `ValidationBehavior` pipeline behavior
      - Register `AddValidatorsFromAssembly` for `TMS.Application`
      - Call `builder.Services.AddInfrastructure(builder.Configuration)`
      - Define `ApplicationAssemblyMarker` empty class in `TMS.Application` for assembly reference
      - Use `GlobalExceptionMiddleware` first in pipeline; add Swagger in dev; `UseHttpsRedirection`, `MapControllers`
    - Create `appsettings.json` with `"MongoDB": { "ConnectionString": "mongodb://localhost:27017", "DatabaseName": "TeacherManagementDb" }`
    - _Requirements: 1.1, 9.5, 9.6_

---

- [ ] 13. Checkpoint — full stack wiring complete
  - Ensure project builds without errors. Run all unit and property-based tests. Ask the user if any adjustments are needed before the final integration test pass.

---

- [ ] 14. Integration tests and domain event consistency
  - [ ]* 14.1 Write integration tests for `TeacherRepository` using Testcontainers
    - Spin up MongoDB container; test `AddAsync` + `GetByIdAsync` round-trip
    - Test `GetPagedAsync` returns correct `TotalCount` with firstName/lastName/email/subjectId filters
    - Test soft-deleted teachers are excluded from `GetByIdAsync` and `GetPagedAsync`
    - _Requirements: 3.3, 4.3–4.6_

  - [ ]* 14.2 Write integration tests for domain event persistence
    - Test that `CreateTeacherCommandHandler` persists a `TeacherCreated` event to `domain_events`
    - Test that `AssignTeacherToCourseCommandHandler` persists a `TeacherAssignedToCourse` event
    - Test that domain events and aggregate are written in the same logical write scope
    - _Requirements: 10.1, 10.5, 10.6_

  - [ ]* 14.3 Write property test for domain event consistency
    - **Property 6: Domain Event Consistency** — for any successful command that modifies a teacher aggregate, the count of domain events in `domain_events` collection increases by at least one
    - **Validates: Requirements 10.6**

  - [ ]* 14.4 Write integration tests for `SubjectRepository` and delete-safety rule
    - Test `IsAssignedToAnyTeacherAsync` returns correct result with/without teacher assignments
    - Test `DeleteSubjectCommandHandler` raises `DomainRuleException` when subject is in use
    - _Requirements: 5.6_

---

- [ ] 15. Final checkpoint — all tests pass
  - Ensure all tests (unit, property, integration) pass. Verify the API builds and all endpoints return correct HTTP status codes for happy-path and error scenarios. Ask the user if any adjustments are needed.

---

## Notes

- Tasks marked with `*` are optional and can be skipped for a faster MVP delivery
- Each task references specific requirements from `requirements.md` for traceability
- Checkpoints (tasks 10, 13, 15) are gates — do not proceed past them with failing tests
- Property tests use `FsCheck.Xunit` `[Property]` attribute; unit tests use xUnit `[Fact]` / `[Theory]`
- All repository integration tests require `Testcontainers.MongoDb` — ensure Docker is running
- `BsonMappingConfiguration.Configure()` must be called once before any MongoDB operations
- `DomainEvents` collection on `Entity` must NOT be persisted in the aggregate BSON document — unmapped in `BsonMappingConfiguration`
- Domain events are persisted to `domain_events` collection by `DomainEventRepository`, not embedded in aggregates
- All error responses include a `correlationId` field (Property 9.6)
- `Teacher.RemoveFromCourse` and `Teacher.RemoveSubject` do not emit domain events per requirements; only the listed events in Requirement 10 are required

---

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1", "1.2"] },
    { "id": 1, "tasks": ["2.1", "2.4"] },
    { "id": 2, "tasks": ["2.2"] },
    { "id": 3, "tasks": ["2.3", "3.2", "3.3", "4.1"] },
    { "id": 4, "tasks": ["3.1"] },
    { "id": 5, "tasks": ["3.4", "3.5", "5.1", "5.2", "6.1", "6.2", "6.3"] },
    { "id": 6, "tasks": ["5.3", "5.4", "5.5", "5.6", "7.1"] },
    { "id": 7, "tasks": ["5.7", "7.2", "7.3", "7.4", "7.5", "7.6", "7.7", "7.8", "8.1", "8.2", "9.1", "9.3", "9.5", "9.6", "9.7", "9.8", "9.9"] },
    { "id": 8, "tasks": ["7.9", "7.10", "8.3", "9.2", "9.4", "11.1"] },
    { "id": 9, "tasks": ["12.1", "12.2", "12.3", "12.4"] },
    { "id": 10, "tasks": ["12.5"] },
    { "id": 11, "tasks": ["14.1", "14.2", "14.3", "14.4"] }
  ]
}
```
