using Microsoft.Extensions.DependencyInjection;

namespace Hoard.Core.Domain.Calculators;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHoardCalculators(this IServiceCollection services)
    {
        services.AddSingleton<IReturnCalculator, HybridReturnCalculator>();
        services.AddSingleton<SimpleReturnCalculator>();
        services.AddSingleton<XirrReturnCalculator>();
        
        return services;
    }
}