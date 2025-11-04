using Hoard.Core.Services.YahooFinance;
using Microsoft.Extensions.DependencyInjection;

namespace Hoard.Core.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHoardServices(this IServiceCollection services)
    {
        services.AddScoped<QuoteService, YahooFinanceClient>();
        services.AddScoped<PriceService, YahooFinanceClient>();

        services.AddScoped<IHoldingsCalculationService, HoldingsCalculationService>();
        
        return services;
    }
}