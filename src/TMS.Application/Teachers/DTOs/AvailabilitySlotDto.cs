namespace TMS.Application.Teachers.DTOs;

public record AvailabilitySlotDto(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);

/*
   AvailabilitySlot = used by the Domain for business rules.
   AvailabilitySlotDto = used by the Application/API to transfer the availability data.
*/