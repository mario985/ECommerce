namespace ECommerce.Common.Application.Errors;

#pragma warning disable CA1716 // Task terminology intentionally names the shared failure value Error.
public sealed record Error(
    string Code,
    string Description,
    ErrorType Type,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null);
#pragma warning restore CA1716
