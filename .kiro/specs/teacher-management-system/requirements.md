# Requirements Document

## Introduction

The Teacher Management System (TMS) is a backend API built with .NET/C#, following CQRS and Domain-Driven Design (DDD) principles, backed by MongoDB. It provides a comprehensive set of capabilities for managing teacher profiles, assigning subjects and courses to teachers, tracking teacher schedules and availability, and querying teacher-related data. The system exposes HTTP endpoints consumed by front-end clients or other services.

---

## Glossary

- **TMS**: Teacher Management System — the software system described in this document.
- **Teacher**: A domain entity representing an individual who teaches one or more subjects or courses.
- **Teacher_Profile**: The aggregate root containing personal and professional information about a Teacher.
- **Subject**: A domain entity representing an academic discipline (e.g., Mathematics, Physics).
- **Course**: A domain entity representing a specific class instance of a Subject, with a schedule and assigned Teacher.
- **Assignment**: A value object representing the association between a Teacher and a Course.
- **Schedule**: A value object representing the set of time slots during which a Teacher is assigned to Courses.
- **Availability**: A value object representing the time slots during which a Teacher is free to be assigned.
- **Command**: A write operation that changes system state (CQRS write side).
- **Query**: A read operation that retrieves data without changing state (CQRS read side).
- **Repository**: The persistence abstraction used to load and save domain aggregates from MongoDB.
- **API**: The HTTP layer that receives client requests and dispatches Commands or Queries.
- **Validator**: The component responsible for validating incoming command or query data.
- **ID**: A system-generated unique identifier (UUID v4) for domain entities.

---

## Requirements

### Requirement 1: Create Teacher Profile

**User Story:** As an administrator, I want to create a new teacher profile, so that the teacher can be managed within the system.

#### Acceptance Criteria

1. WHEN a valid CreateTeacher command is received, THE TMS SHALL create a new Teacher_Profile with a system-generated ID and persist it to the Repository.
2. WHEN a CreateTeacher command is received, THE Validator SHALL verify that the `firstName`, `lastName`, and `email` fields are present and non-empty before the command is processed.
3. WHEN a CreateTeacher command is received, THE Validator SHALL verify that the `email` field conforms to a valid email address format before the command is processed.
4. WHEN a CreateTeacher command contains an `email` that already exists in the Repository, THE TMS SHALL return a conflict error indicating the email is already in use.
5. WHEN a CreateTeacher command is processed successfully, THE TMS SHALL return the newly created Teacher_Profile including its generated ID.
6. IF the Repository is unavailable during a CreateTeacher command, THEN THE TMS SHALL return a service unavailable error and SHALL NOT partially persist data.

---

### Requirement 2: Update Teacher Profile

**User Story:** As an administrator, I want to update an existing teacher's profile information, so that the teacher's records remain accurate.

#### Acceptance Criteria

1. WHEN a valid UpdateTeacher command is received with an existing teacher ID, THE TMS SHALL update the specified Teacher_Profile fields and persist the changes to the Repository.
2. WHEN an UpdateTeacher command is received, THE Validator SHALL verify that at least one updatable field (`firstName`, `lastName`, `email`, `phoneNumber`, `dateOfBirth`, `address`) is present.
3. WHEN an UpdateTeacher command contains an `email` that belongs to a different Teacher in the Repository, THE TMS SHALL return a conflict error.
4. WHEN an UpdateTeacher command references a teacher ID that does not exist in the Repository, THE TMS SHALL return a not-found error.
5. WHEN an UpdateTeacher command is processed successfully, THE TMS SHALL return the updated Teacher_Profile.

---

### Requirement 3: Delete Teacher Profile

**User Story:** As an administrator, I want to delete a teacher profile, so that records for teachers who are no longer active can be removed.

#### Acceptance Criteria

1. WHEN a DeleteTeacher command is received with an existing teacher ID, THE TMS SHALL mark the Teacher_Profile as deleted (soft delete) and persist the change to the Repository.
2. WHEN a DeleteTeacher command references a teacher ID that does not exist in the Repository, THE TMS SHALL return a not-found error.
3. WHILE a Teacher_Profile is marked as deleted, THE TMS SHALL exclude it from all Query results by default.
4. WHEN a DeleteTeacher command is received for a Teacher_Profile that has active Course Assignments, THE TMS SHALL return a validation error indicating the teacher has active assignments that must be removed first.

---

### Requirement 4: View Teacher Profile

**User Story:** As an administrator or client, I want to retrieve a teacher's profile, so that I can view their details.

#### Acceptance Criteria

1. WHEN a GetTeacherById query is received with an existing teacher ID, THE TMS SHALL return the full Teacher_Profile for that ID.
2. WHEN a GetTeacherById query references a teacher ID that does not exist or is soft-deleted, THE TMS SHALL return a not-found error.
3. WHEN a GetAllTeachers query is received, THE TMS SHALL return a paginated list of all active Teacher_Profiles.
4. WHEN a GetAllTeachers query includes filter parameters (`firstName`, `lastName`, `email`, `subjectId`), THE TMS SHALL return only Teacher_Profiles matching all provided filters.
5. WHEN a GetAllTeachers query includes `pageNumber` and `pageSize` parameters, THE TMS SHALL return results for the requested page using the provided page size.
6. IF a GetAllTeachers query omits `pageSize`, THEN THE TMS SHALL apply a default page size of 20.

---

### Requirement 5: Manage Subjects

**User Story:** As an administrator, I want to create and manage subjects, so that teachers can be assigned to specific academic disciplines.

#### Acceptance Criteria

1. WHEN a valid CreateSubject command is received, THE TMS SHALL create a new Subject with a system-generated ID and persist it to the Repository.
2. WHEN a CreateSubject command is received, THE Validator SHALL verify that the `name` field is present, non-empty, and does not exceed 200 characters.
3. WHEN a CreateSubject command contains a `name` that already exists in the Repository, THE TMS SHALL return a conflict error.
4. WHEN a GetAllSubjects query is received, THE TMS SHALL return the complete list of active Subjects.
5. WHEN a DeleteSubject command is received with an existing subject ID, THE TMS SHALL mark the Subject as deleted and persist the change to the Repository.
6. WHEN a DeleteSubject command references a subject ID that is currently assigned to at least one Teacher, THE TMS SHALL return a validation error indicating the subject is in use.

---

### Requirement 6: Assign Subjects to Teachers

**User Story:** As an administrator, I want to assign one or more subjects to a teacher, so that the system knows which disciplines a teacher is qualified to teach.

#### Acceptance Criteria

1. WHEN a valid AssignSubjectToTeacher command is received, THE TMS SHALL create an Assignment linking the specified Teacher and Subject and persist it to the Repository.
2. WHEN an AssignSubjectToTeacher command references a teacher ID that does not exist, THE TMS SHALL return a not-found error for the Teacher.
3. WHEN an AssignSubjectToTeacher command references a subject ID that does not exist, THE TMS SHALL return a not-found error for the Subject.
4. WHEN an AssignSubjectToTeacher command references a Teacher and Subject combination that already has an active Assignment, THE TMS SHALL return a conflict error.
5. WHEN a RemoveSubjectFromTeacher command is received with a valid teacher ID and subject ID, THE TMS SHALL remove the Assignment from the Repository.
6. WHEN a GetSubjectsByTeacher query is received with a valid teacher ID, THE TMS SHALL return all Subjects currently assigned to that Teacher.

---

### Requirement 7: Manage Teacher Availability

**User Story:** As an administrator, I want to set and update a teacher's availability, so that course scheduling can be planned around when each teacher is free.

#### Acceptance Criteria

1. WHEN a valid SetTeacherAvailability command is received, THE TMS SHALL persist the provided Availability time slots for the specified Teacher, replacing any previously stored Availability.
2. WHEN a SetTeacherAvailability command is received, THE Validator SHALL verify that each availability slot contains a valid `dayOfWeek` (Monday–Sunday), a `startTime`, and an `endTime`.
3. WHEN a SetTeacherAvailability command contains a slot where `startTime` is not earlier than `endTime`, THE Validator SHALL return a validation error for that slot.
4. WHEN a SetTeacherAvailability command references a teacher ID that does not exist, THE TMS SHALL return a not-found error.
5. WHEN a GetTeacherAvailability query is received with a valid teacher ID, THE TMS SHALL return all current Availability slots for that Teacher.
6. WHEN a GetAvailableTeachers query is received with a `dayOfWeek` and a time range, THE TMS SHALL return all Teachers whose Availability includes the specified day and overlaps the specified time range.

---

### Requirement 8: Manage Teacher Schedule (Course Assignments)

**User Story:** As an administrator, I want to assign a teacher to a specific course with a defined time slot, so that the school schedule is maintained.

#### Acceptance Criteria

1. WHEN a valid AssignTeacherToCourse command is received, THE TMS SHALL create a Schedule entry linking the Teacher, Course, and time slot and persist it to the Repository.
2. WHEN an AssignTeacherToCourse command is received, THE Validator SHALL verify that the `teacherId`, `courseId`, `dayOfWeek`, `startTime`, and `endTime` fields are all present.
3. WHEN an AssignTeacherToCourse command is received and the specified Teacher already has a Schedule entry that overlaps the provided time slot on the same `dayOfWeek`, THE TMS SHALL return a scheduling conflict error.
4. WHEN an AssignTeacherToCourse command references a teacher ID that does not exist, THE TMS SHALL return a not-found error.
5. WHEN an AssignTeacherToCourse command references a course ID that does not exist, THE TMS SHALL return a not-found error.
6. WHEN a RemoveTeacherFromCourse command is received with a valid teacher ID and course ID, THE TMS SHALL remove the corresponding Schedule entry from the Repository.
7. WHEN a GetTeacherSchedule query is received with a valid teacher ID, THE TMS SHALL return all Schedule entries for that Teacher.
8. WHEN a GetTeacherSchedule query includes an optional `dayOfWeek` filter, THE TMS SHALL return only Schedule entries matching that day.

---

### Requirement 9: Input Validation and Error Handling

**User Story:** As a client, I want the API to return clear, structured error responses, so that I can handle errors programmatically.

#### Acceptance Criteria

1. WHEN any Command or Query receives a request with missing required fields, THE Validator SHALL return an HTTP 400 response containing a structured error body listing each invalid field and the reason.
2. WHEN any Command references an entity ID that does not exist, THE TMS SHALL return an HTTP 404 response with a structured error body identifying the missing entity.
3. WHEN any Command violates a uniqueness constraint, THE TMS SHALL return an HTTP 409 response with a structured error body describing the conflict.
4. WHEN any Command violates a business rule (e.g., scheduling conflict, active assignments on delete), THE TMS SHALL return an HTTP 422 response with a structured error body describing the violated rule.
5. IF an unhandled exception occurs during request processing, THEN THE TMS SHALL return an HTTP 500 response and SHALL log the exception details including a correlation ID.
6. THE TMS SHALL include a `correlationId` field in every error response body to enable request tracing.

---

### Requirement 10: Audit and Domain Events

**User Story:** As a system integrator, I want the system to emit domain events when significant state changes occur, so that other services can react to changes.

#### Acceptance Criteria

1. WHEN a Teacher_Profile is created, THE TMS SHALL emit a `TeacherCreated` domain event containing the teacher ID, full name, and email.
2. WHEN a Teacher_Profile is updated, THE TMS SHALL emit a `TeacherUpdated` domain event containing the teacher ID and the fields that changed.
3. WHEN a Teacher_Profile is soft-deleted, THE TMS SHALL emit a `TeacherDeleted` domain event containing the teacher ID.
4. WHEN a Subject is assigned to a Teacher, THE TMS SHALL emit a `SubjectAssignedToTeacher` domain event containing the teacher ID and subject ID.
5. WHEN a Teacher is assigned to a Course, THE TMS SHALL emit a `TeacherAssignedToCourse` domain event containing the teacher ID, course ID, and time slot.
6. THE TMS SHALL persist all domain events to a dedicated events collection in MongoDB as part of the same write operation that persists the aggregate change.
