using TMS.Application;
using TMS.Application.Common.Behaviors;
using TMS.Infrastructure;
using TMS.Infrastructure.Persistence;
using TMS.Infrastructure.Persistence.Mappings;
using FluentValidation;
using MediatR;

// Must be called before any MongoDB driver operations
/*
   MongoDB needs to know, "How should my C# domain objects be converted into MongoDB BSON documents?"


*/
BsonMappingConfiguration.Configure();

var builder = WebApplication.CreateBuilder(args);

// ── MVC + Swagger ────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── MediatR ──────────────────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

// ── FluentValidation ─────────────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);

// ── Infrastructure (MongoDB context + repositories) ──────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
// TODO: app.UseMiddleware<GlobalExceptionMiddleware>(); // Task 12.1

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// ── Ensure MongoDB indexes exist ──────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    // Before the API starts accepting requests, make sure the required MongoDB indexes exist.
    var mongoDbContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    await mongoDbContext.EnsureIndexesAsync();
}

app.Run();


/*
   Infrastructure is responsible for persistence.
   Application coordinates use cases.
   Domain contains business rules.

   Startup sequence:
   1.Configure MongoDB BSON mappings.
   2.Create WebApplication builder.
   3.Register Controllers.
   4.Register Swagger.
   5.Register MediatR.
   6.Register FluentValidation.
   7.Register Infrastructure.
   8.Build application.
   9.Configure middleware.
   10.Configure endpoints.
   11.Ensure MongoDB indexes.
   12.Run API.
*/
