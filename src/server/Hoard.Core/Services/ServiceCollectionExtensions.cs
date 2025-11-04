using Hoard.Core.Services.YahooFinance;
using Microsoft.Extensions.DependencyInjection;

namespace Hoard.Core.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHoardServices(this IServiceCollection services)
    {
        services.AddScoped<IQuoteService, YahooFinanceClient>();
        services.AddScoped<IPriceService, YahooFinanceClient>();

        services.AddScoped<IHoldingsRecalculationService, HoldingsRecalculationService>();
        
        return services;
    }
}