using System.Reflection;
using ECommerce.Common.Application.Errors;
using FluentValidation;
using MediatR;

namespace ECommerce.Common.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        IValidator<TRequest>[] applicableValidators = validators.ToArray();
        bool resultResponse = typeof(TResponse) == typeof(Result) ||
            (typeof(TResponse).IsGenericType &&
             typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>));
        if (applicableValidators.Length == 0 || !resultResponse)
        {
            return await next(cancellationToken);
        }

        FluentValidation.Results.ValidationResult[] results = await Task.WhenAll(
            applicableValidators.Select(validator => validator.ValidateAsync(request, cancellationToken)));
        FluentValidation.Results.ValidationFailure[] failures = results
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .GroupBy(failure => failure.PropertyName, StringComparer.OrdinalIgnoreCase)
            .SelectMany(group => group)
            .ToArray();
        if (failures.Length == 0)
        {
            return await next(cancellationToken);
        }

        Dictionary<string, string[]> errors = failures
            .GroupBy(failure => NormalizePropertyName(failure.PropertyName), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray(),
                StringComparer.OrdinalIgnoreCase);
        Error error = new(
            "Api.Validation",
            "One or more validation errors occurred.",
            ErrorType.Validation,
            errors);
        return CreateFailure(error);
    }

    private static string NormalizePropertyName(string propertyName)
    {
        int separator = propertyName.LastIndexOf('.');
        return separator >= 0 ? propertyName[(separator + 1)..] : propertyName;
    }

    private static TResponse CreateFailure(Error error)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            Type valueType = typeof(TResponse).GetGenericArguments()[0];
            MethodInfo method = typeof(Result)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(method => method.Name == nameof(Result.Failure)
                    && method.IsGenericMethodDefinition
                    && method.GetParameters().Length == 1
                    && method.GetParameters()[0].ParameterType == typeof(Error))
                .MakeGenericMethod(valueType);
            return (TResponse)method.Invoke(null, [error])!;
        }

        throw new InvalidOperationException(
            $"ValidationBehavior requires a Result response, but '{typeof(TResponse).Name}' was returned.");
    }
}
