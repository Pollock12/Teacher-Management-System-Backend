using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TMS.Domain.Repositories;
using TMS.Infrastructure.Persistence;
using TMS.Infrastructure.Persistence.Repositories;

namespace TMS.Infrastructure;

/// <summary>
/// Extension methods for registering all Infrastructure-layer services
/// with the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the MongoDB context and all repository implementations.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration (used to bind <c>"MongoDB"</c> section).</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    
    /*
        Its job is to register services with .NET's DI container
        What is a DI container?
        Think of the DI container as a box that knows how to create objects.
    */
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        /*
           services -> This is where you register your dependencies
           configuration -> This contains configuration from things like appsettings.json, environment variables and configuration files.


        */
        // ── Bind MongoDbSettings from the "MongoDB" configuration section ──
        //appsettings.json -> "MongoDB" -> Bind() -> MongoDbSettings
        var mongoDbSettings = new MongoDbSettings();
        configuration.GetSection("MongoDB").Bind(mongoDbSettings);

        // ── Register MongoDbContext as a singleton ─────────────────────────
        // A single MongoClient/IMongoDatabase is safe to share across the
        // entire application lifetime (the MongoDB driver is thread-safe).

        // mongoDbSettings -> This tells .NET keep this same MongoDbSettings object and give it whenever someone needs it.
        services.AddSingleton(mongoDbSettings);
        // MongoDbContext -> It tells .NET when someone asks for MongoDbContext, create one and reuse the same instance.
        // ***** Singleton means create only one instance for the application's lifetime.
        services.AddSingleton<MongoDbContext>();

        // ── Register repositories as scoped ───────────────────────────────
        // Scoped lifetime ties each repository instance to a single HTTP
        // request, which is the standard practice for repositories.
        // Whenever the application asks for ITeacherRepository, create a TeacherRepository
        // ***** Scoped means create one repository instance for each HTTP request.

        /*
           Need ITeacherRepository -> Create TeacherRepository -> TeacherRepository needs MongoDbContext -> Get MongoDbContext -> MongoDbContext needs MongoDbSettings -> Get MongoDbSettings -> Everything created
           This is the real power of Dependency Injection.

           Think of this -> Hey .NET, whenever somebody asks for ITeacherRepository, give them a TeacherRepository
        */
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IDomainEventRepository, DomainEventRepository>();

        return services;

        // my entire Infrastructure setup is basically a configuration map telling .NET how all my classed are connected.
    }
}

/*
    Why Singleton for MongoDbContext?
    MongoClient (inside MongoDbContext) is designed to be created once and reused.
    Creating multiple instances wastes resources and can cause connection pool issues.

    Why Scoped for repositories?
    Each HTTP request gets its own repository instance.
    This is the standard pattern: repositories are lightweight wrappers, so creating
    one per request is cheap and avoids any accidental state sharing between requests.

    AddInfrastructure() is called from Program.cs (TMS.API layer) like this:
        builder.Services.AddInfrastructure(builder.Configuration);
*/
