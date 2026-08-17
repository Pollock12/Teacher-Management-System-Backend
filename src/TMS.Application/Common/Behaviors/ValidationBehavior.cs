using FluentValidation;
using MediatR;

namespace TMS.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs all registered FluentValidation validators
/// for the incoming request. Throws <see cref="ValidationException"/> if any
/// validation failures are found, preventing the handler from being reached.
/// Satisfies Requirement 9.1.
/// </summary>


/*
   This class is a MediatR Pipeline Behavior that performs validation before your request reaches its Handler.
   API request -> MediatR -> ValidationBehavior -> Valid -> Handler -> Repository -> MongoDB
   Instead of putting validation inside every Handler, your validationBehavior automatically checks it.
   IPipelineBehavior means I want to execute some code before/after a MediatR Handler.
   TRequest -> The request coming into MediatR (CreateTeacherCommand)
   TResponse -> The response returned by the Handler (TeacherDTO)

   MediatR -> Instead of your API Controller directly calling a Handler/Service, it sends a request through MediatR.

   Whithout MediatR : TeacherController -> TeacherService -> TeacherRepository
   With MediatR : TeacherController -> MediatR -> CreateTeacherHandler -> TeacherRepository

   FluentValidation -> is a library used to define validation rules for your requests.
*/
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        // Run all validators concurrently and collect every failure
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
