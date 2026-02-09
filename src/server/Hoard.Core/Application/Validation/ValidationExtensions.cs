using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Core.Application.Validation;

public static class ValidationExtensions
{
    public const string EntityIdKey = "EntityId";

    public static async Task<ValidationProblemDetails?> ValidateAndGetProblemsAsync<T>(
        this IValidator<T> validator,
        T instance,
        int? entityId = null,
        CancellationToken cancellationToken = default)
    {
        var context = new ValidationContext<T>(instance);

        if (entityId.HasValue)
        {
            context.RootContextData[EntityIdKey] = entityId.Value;
        }

        var result = await validator.ValidateAsync(context, cancellationToken);

        if (result.IsValid)
        {
            return null;
        }

        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return new ValidationProblemDetails(errors);
    }
}
