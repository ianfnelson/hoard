using Clarion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hoard.Core.Infrastructure.Integrations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIntegrations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IQuoteService, YahooFinance>();
        services.AddScoped<IPriceService, YahooFinance>();
        services.AddScoped<IPortfolioValuationsPdfService, PortfolioValuationsPdfService>();
        services.AddScoped<IClarionClient>(_ => ClarionClient.Create());

        // Azure Blob Storage
        var blobConnectionString = configuration.GetConnectionString("AzureBlobStorage")
            ?? throw new InvalidOperationException("Azure Blob Storage connection string not configured");
        var containerName = configuration["BlobStorage:ContainerName"] ?? "documents";

        services.AddSingleton<IBlobStorageService>(sp =>
            new AzureBlobStorage(blobConnectionString, containerName));

        // Pushover Notifications
        var pushoverToken = configuration["Pushover:Token"]
            ?? throw new InvalidOperationException("Pushover token not configured");
        var pushoverUser = configuration["Pushover:User"]
            ?? throw new InvalidOperationException("Pushover user not configured");

        services.AddHttpClient<INotificationService, Pushover>((httpClient, sp) =>
            new Pushover(httpClient, pushoverToken, pushoverUser));

        return services;
    }
}