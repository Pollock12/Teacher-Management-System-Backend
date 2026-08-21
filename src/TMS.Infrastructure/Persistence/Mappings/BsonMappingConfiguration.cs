using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using TMS.Domain.Aggregates.Courses;
using TMS.Domain.Aggregates.Subjects;
using TMS.Domain.Aggregates.Teachers;
using TMS.Domain.Common;
using TMS.Domain.ValueObjects;

namespace TMS.Infrastructure.Persistence.Mappings;

/// <summary>
/// Centralises all BSON serialiser and class-map registrations for the TMS domain.
/// Call <see cref="Configure"/> once at application startup, before any MongoDB
/// operation is executed.
/// </summary>

/*
Here is how my TMS domain classes should be converted into MongoDB BSON documents,
and how MongoDB documents should be converted back into my C# objects.
MongoDB stores data in a format called BSON(Binary JSON)

Why do we need BsonMappingConfiguration?
=> Some of your C# types are straightforward : string,Guid,DateTime,bool.
   But my project also uses: TimeOnly,DateOnly,Value Objetcs, private fields, domain events.
   MongoDB doesn't automatically know exactly how i want all of these represented.
*/

public static class BsonMappingConfiguration
{
    /// <summary>
    /// Registers all custom serialisers and class maps.
    /// Safe to call multiple times — each registration is guarded by an
    /// <c>IsClassMapRegistered</c> / try-catch idempotency check.
    /// </summary>
    public static void Configure()
    {
        RegisterSerializers(); // Configure special data types (Guid, TimeOnly, DateOnly)
        RegisterEntityMap(); // Configure Entity
        RegisterTeacherMap(); //Configure Teacher
        RegisterSubjectMap(); // Configure Subject
        RegisterCourseMap(); // Configure Course
        RegisterAvailabilitySlotMap(); // Configure AvailabilitySlot
        RegisterScheduleEntryMap(); // Configure ScheduleEntry
        RegisterSubjectAssignmentMap(); // Configure SubjectAssignment
    }

    // ── Custom serialiser registrations ────────────────────────────────────

    private static void RegisterSerializers()
    {
        // GuidSerializer — store GUIDs as standard UUID strings
        try
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }
        catch (BsonSerializationException)
        {
            // Already registered — safe to ignore
        }

        // TimeOnly — serialised as "HH:mm:ss" string
        try
        {
            BsonSerializer.RegisterSerializer(new TimeOnlySerializer());
        }
        catch (BsonSerializationException)
        {
            // Already registered — safe to ignore
        }

        // DateOnly — serialised as "yyyy-MM-dd" string
        try
        {
            BsonSerializer.RegisterSerializer(new DateOnlySerializer());
        }
        catch (BsonSerializationException)
        {
            // Already registered — safe to ignore
        }
    }

    // ── Class map registrations ─────────────────────────────────────────────

    private static void RegisterEntityMap()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(Entity)))
            return;

        BsonClassMap.RegisterClassMap<Entity>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(e => e.Id).SetElementName("_id");
            cm.MapMember(e => e.CreatedAt).SetElementName("createdAt");
            cm.MapMember(e => e.UpdatedAt).SetElementName("updatedAt");
            cm.SetIgnoreExtraElements(true);

            // DomainEvents lives on Entity — unmap it once here for all subclasses
            cm.UnmapMember(e => e.DomainEvents);
            cm.UnmapField("_domainEvents");
        });
    }

    /*
      Teacher contains private fields( _subjectAssignments, _availabilitySlots, _scheduleEntities)
      These are private fields.Normally MongoDB mapping primarily worked with public members, so you explicitly tell it about these fields.
      Domain events are not stored inside Teacher.
      Private field -> _domain model controls it -> MongoDB still needs to save it -> MapField()
    */

    private static void RegisterTeacherMap()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(Teacher)))
            return;

        BsonClassMap.RegisterClassMap<Teacher>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);

            // DomainEvents / _domainEvents are already unmapped on the Entity base class map.
            // Do NOT call UnmapMember/UnmapField here — that would pass a MemberInfo
            // belonging to Entity into a Teacher class map, causing ArgumentOutOfRangeException.

            // Map private backing fields so MongoDB can hydrate them on load.
            cm.MapField("_subjectAssignments").SetElementName("subjectAssignments");
            cm.MapField("_availabilitySlots").SetElementName("availabilitySlots");
            cm.MapField("_scheduleEntries").SetElementName("scheduleEntries");
        });
    }

    private static void RegisterSubjectMap()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(Subject)))
            return;

        BsonClassMap.RegisterClassMap<Subject>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
            // DomainEvents / _domainEvents already unmapped on Entity base class map.
        });
    }

    private static void RegisterCourseMap()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(Course)))
            return;

        BsonClassMap.RegisterClassMap<Course>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
            // DomainEvents / _domainEvents already unmapped on Entity base class map.
        });
    }

    /*
       AutoMap() -> MongoDB, automatically map my properties.
       MapCreator() -> MongoDB, when creating this object, use this constructor
    */

    private static void RegisterAvailabilitySlotMap()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(AvailabilitySlot)))
            return;

        BsonClassMap.RegisterClassMap<AvailabilitySlot>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
            cm.MapCreator(a => new AvailabilitySlot(a.DayOfWeek, a.StartTime, a.EndTime));
        });
    }

    private static void RegisterScheduleEntryMap()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(ScheduleEntry)))
            return;

        BsonClassMap.RegisterClassMap<ScheduleEntry>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
            cm.MapCreator(s => new ScheduleEntry(s.CourseId, s.DayOfWeek, s.StartTime, s.EndTime));
        });
    }

    private static void RegisterSubjectAssignmentMap()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(SubjectAssignment)))
            return;

        BsonClassMap.RegisterClassMap<SubjectAssignment>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
            // SubjectAssignment constructor sets AssignedAt = DateTime.UtcNow, but when
            // deserialising from MongoDB we need to restore the persisted value.
            // We use MapCreator with subjectId only and then rely on AutoMap having mapped
            // AssignedAt as a readable property — MongoDB will set it via the member map
            // after construction.  Since the property has no setter, we need to map it
            // through the constructor as a creator argument instead.
            cm.MapCreator(sa => new SubjectAssignment(sa.SubjectId));
        });
    }
}

// ── Custom serialisers ──────────────────────────────────────────────────────

/// <summary>
/// Serialises <see cref="TimeOnly"/> values as <c>"HH:mm:ss"</c> strings.
/// </summary>
public sealed class TimeOnlySerializer : SerializerBase<TimeOnly>
{
    private const string Format = "HH:mm:ss";

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TimeOnly value)
    {
        context.Writer.WriteString(value.ToString(Format));
    }

    public override TimeOnly Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var raw = context.Reader.ReadString();
        return TimeOnly.ParseExact(raw, Format);
    }
}

/// <summary>
/// Serialises <see cref="DateOnly"/> values as <c>"yyyy-MM-dd"</c> strings.
/// </summary>
public sealed class DateOnlySerializer : SerializerBase<DateOnly>
{
    private const string Format = "yyyy-MM-dd";

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateOnly value)
    {
        context.Writer.WriteString(value.ToString(Format));
    }

    public override DateOnly Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var raw = context.Reader.ReadString();
        return DateOnly.ParseExact(raw, Format);
    }
}

/*
   BsonMappingConfiguration is the translation rule between DDD domain model and MongoDB.
   It answers questions like:
     How should TimeOnly be stored?
     How should DateOnly be stored?
     How should private fields be persisted?
     How should MongoDB recreate my Value Objects?
     Which domain properties should not be persisted?
    Thats why this code belongs in TMS.Infrastructure, not in my Domain Layer
*/