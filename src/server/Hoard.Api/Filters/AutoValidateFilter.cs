using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Hoard.Api.Filters;

public class AutoValidateFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public AutoValidateFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Auto-validate any DTOs in action arguments
        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg == null) continue;

            var argType = arg.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argType);
            var validator = _serviceProvider.GetService(validatorType);

            if (validator != null)
            {
                var validateMethod = validatorType.GetMethod("ValidateAsync",
                    new[] { argType, typeof(CancellationToken) });

                if (validateMethod != null)
                {
                    var validationTask = validateMethod.Invoke(
                        validator,
                        new[] { arg, context.HttpContext.RequestAborted }
                    );

                    if (validationTask != null)
                    {
                        await (dynamic)validationTask;
                        var result = ((dynamic)validationTask).Result;

                        if (!result.IsValid)
                        {
                            var errors = ((IEnumerable<dynamic>)result.Errors)
                                .GroupBy(e => (string)e.PropertyName)
                                .ToDictionary(
                                    g => g.Key,
                                    g => g.Select(e => (string)e.ErrorMessage).ToArray()
                                );

                            context.Result = new BadRequestObjectResult(
                                new ValidationProblemDetails(errors)
                            );
                            return;
                        }
                    }
                }
            }
        }

        await next();
    }
}
